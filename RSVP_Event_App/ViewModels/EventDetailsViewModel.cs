using System.Collections.ObjectModel;
using System.Windows.Input;
using RSVP_Event_App.Data;
using RSVP_Event_App.Models;
using RSVP_Event_App.Services;
using RSVP_Event_App.Views;

namespace RSVP_Event_App.ViewModels;

public class EventDetailsViewModel : BaseViewModel, IQueryAttributable
{
    public const string EventIdKey = "EventId";

    private readonly RsvpDatabase _database;
    private int _eventId;

    public EventDetailsViewModel(RsvpDatabase database)
    {
        _database = database;
        RsvpCommand = new Command(async () => await OpenRsvpAsync());
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(EventIdKey, out var id))
            _eventId = (int)id;
    }

    public ObservableCollection<string> Attendees { get; } = new();

    private Event? _event;
    public Event? Event { get => _event; set => SetProperty(ref _event, value); }

    private bool _hasNoAttendees;
    public bool HasNoAttendees { get => _hasNoAttendees; set => SetProperty(ref _hasNoAttendees, value); }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (SetProperty(ref _statusMessage, value))
                OnPropertyChanged(nameof(HasStatus));
        }
    }

    public bool HasStatus => !string.IsNullOrEmpty(StatusMessage);

    private bool _canRsvp;
    public bool CanRsvp { get => _canRsvp; set => SetProperty(ref _canRsvp, value); }

    public ICommand RsvpCommand { get; }

    // Reloads every time the page appears so new RSVPs show up immediately
    public async Task LoadAsync()
    {
        List<Rsvp> rsvps;
        try
        {
            Event = await _database.GetEventAsync(_eventId);
            rsvps = Event is null ? new List<Rsvp>() : await _database.GetRsvpsForEventAsync(_eventId);
        }
        catch (ServiceUnavailableException ex)
        {
            await Shell.Current.DisplayAlert("Connection Problem", ex.Message, "OK");
            await Shell.Current.GoToAsync("..");
            return;
        }

        if (Event is null)
        {
            await Shell.Current.DisplayAlert("Not Found", "This event no longer exists.", "OK");
            await Shell.Current.GoToAsync("..");
            return;
        }

        Attendees.Clear();
        foreach (var rsvp in rsvps)
            Attendees.Add(rsvp.Name);
        HasNoAttendees = rsvps.Count == 0;

        await UpdateRsvpStateAsync();
    }

    private async Task UpdateRsvpStateAsync()
    {
        bool alreadyAttending = false;
        try
        {
            alreadyAttending = Session.CurrentUser is { } user &&
                await _database.HasRsvpAsync(_eventId, user.Email);
        }
        catch (ServiceUnavailableException)
        {
            // Keep the state based on the event's own rules; submitting will report the problem
        }

        // Attending comes first: someone on the list should see that even if the event has since filled up
        string? reason = alreadyAttending ? "You're attending this event."
                       : Event!.IsRsvpClosed ? "RSVP is closed for this event."
                       : Event.IsFull ? "This event is full."
                       : null;

        CanRsvp = reason is null;
        StatusMessage = reason ?? string.Empty;
    }

    private async Task OpenRsvpAsync()
    {
        await Shell.Current.GoToAsync(nameof(RsvpPage),
            new Dictionary<string, object> { [RsvpViewModel.EventKey] = Event! });
    }
}
