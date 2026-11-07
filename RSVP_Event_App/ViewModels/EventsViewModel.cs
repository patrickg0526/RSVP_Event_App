using System.Collections.ObjectModel;
using System.Windows.Input;
using RSVP_Event_App.Data;
using RSVP_Event_App.Models;
using RSVP_Event_App.Services;
using RSVP_Event_App.Views;

namespace RSVP_Event_App.ViewModels;

public enum EventFilter { All, Attending, Hosting }

public class EventsViewModel : BaseViewModel
{
    private readonly RsvpDatabase _database;
    private int _sessionVersion = -1;

    public EventsViewModel(RsvpDatabase database)
    {
        _database = database;
        FilterCommand = new Command<EventFilter>(async filter => await ApplyFilterAsync(filter));
        OpenEventCommand = new Command(async () => await OpenSelectedEventAsync());
        AddEventCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(AddEventPage)));
        AccountCommand = new Command(async () => await LeaveAsync());
    }

    public ObservableCollection<Event> Events { get; } = new();

    private Event? _selectedEvent;
    public Event? SelectedEvent { get => _selectedEvent; set => SetProperty(ref _selectedEvent, value); }

    private string _welcomeText = string.Empty;
    public string WelcomeText { get => _welcomeText; set => SetProperty(ref _welcomeText, value); }

    private string _accountText = string.Empty;
    public string AccountText { get => _accountText; set => SetProperty(ref _accountText, value); }

    private bool _isLoggedIn;
    public bool IsLoggedIn { get => _isLoggedIn; set => SetProperty(ref _isLoggedIn, value); }

    private EventFilter _filter = EventFilter.All;
    public EventFilter Filter
    {
        get => _filter;
        private set
        {
            if (SetProperty(ref _filter, value))
            {
                OnPropertyChanged(nameof(IsAllSelected));
                OnPropertyChanged(nameof(IsAttendingSelected));
                OnPropertyChanged(nameof(IsHostingSelected));
            }
        }
    }

    public bool IsAllSelected => Filter == EventFilter.All;
    public bool IsAttendingSelected => Filter == EventFilter.Attending;
    public bool IsHostingSelected => Filter == EventFilter.Hosting;

    public ICommand FilterCommand { get; }
    public ICommand OpenEventCommand { get; }
    public ICommand AddEventCommand { get; }
    public ICommand AccountCommand { get; }

    // Called by the View each time the page appears
    public async Task LoadAsync()
    {
        bool isGuest = Session.IsGuest;

        // A new login shows the events the user is attending; guests can only view all events
        if (_sessionVersion != Session.Version)
        {
            _sessionVersion = Session.Version;
            Filter = isGuest ? EventFilter.All : EventFilter.Attending;
        }

        WelcomeText = isGuest ? "Browsing as Guest" : $"Welcome, {Session.CurrentUser!.Name.Split(' ')[0]}!";
        AccountText = isGuest ? "Log In" : "Log Out";
        IsLoggedIn = !isGuest;

        await LoadEventsAsync();
    }

    private async Task ApplyFilterAsync(EventFilter filter)
    {
        Filter = filter;
        await LoadEventsAsync();
    }

    private async Task LoadEventsAsync()
    {
        try
        {
            List<Event> events = Filter switch
            {
                EventFilter.Attending => await _database.GetAttendingEventsAsync(Session.CurrentUser!),
                EventFilter.Hosting => await _database.GetHostingEventsAsync(Session.CurrentUser!.Id),
                _ => await _database.GetAllEventsAsync()
            };

            Events.Clear();
            foreach (var ev in events)
                Events.Add(ev);
        }
        catch (ServiceUnavailableException ex)
        {
            Events.Clear();
            await Shell.Current.DisplayAlert("Connection Problem", ex.Message, "OK");
        }
    }

    private async Task OpenSelectedEventAsync()
    {
        if (SelectedEvent is not { } selected)
            return;

        SelectedEvent = null;
        await Shell.Current.GoToAsync(nameof(EventDetailsPage),
            new Dictionary<string, object> { [EventDetailsViewModel.EventIdKey] = selected.Id });
    }

    // Log out (or, for a guest, return to the login screen)
    private async Task LeaveAsync()
    {
        Session.SignOut();
        await Shell.Current.GoToAsync("//login");
    }
}
