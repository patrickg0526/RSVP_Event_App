using RSVP_Event_App.Models;
using RSVP_Event_App.Services;

namespace RSVP_Event_App.Views;

public partial class EventsPage : ContentPage
{
    private enum EventFilter { All, Attending, Hosting }

    private EventFilter _filter = EventFilter.All;
    private bool _wasGuest = true;

    public EventsPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        bool isGuest = Session.IsGuest;

        // Upon login show the events the user is attending; guests can only view all events
        if (isGuest)
            _filter = EventFilter.All;
        else if (_wasGuest)
            _filter = EventFilter.Attending;
        _wasGuest = isGuest;

        lblWelcome.Text = isGuest
            ? "Browsing as Guest"
            : $"Welcome, {Session.CurrentUser!.Name.Split(' ')[0]}!";
        tbiAccount.Text = isGuest ? "Log In" : "Log Out";
        btnAttending.IsVisible = !isGuest;
        btnHosting.IsVisible = !isGuest;
        btnAddEvent.IsVisible = !isGuest;

        await LoadEventsAsync();
    }

    private async Task LoadEventsAsync()
    {
        List<Event> events = _filter switch
        {
            EventFilter.Attending => await App.Database.GetAttendingEventsAsync(Session.CurrentUser!),
            EventFilter.Hosting => await App.Database.GetHostingEventsAsync(Session.CurrentUser!.Id),
            _ => await App.Database.GetAllEventsAsync()
        };

        cvEvents.ItemsSource = events;
        HighlightFilter();
    }

    private void HighlightFilter()
    {
        var selected = Application.Current!.RequestedTheme == AppTheme.Dark
            ? (Color)Application.Current.Resources["PrimaryDark"]
            : (Color)Application.Current.Resources["Primary"];
        var unselected = (Color)Application.Current.Resources["Gray500"];

        btnAll.BackgroundColor = _filter == EventFilter.All ? selected : unselected;
        btnAttending.BackgroundColor = _filter == EventFilter.Attending ? selected : unselected;
        btnHosting.BackgroundColor = _filter == EventFilter.Hosting ? selected : unselected;
    }

    private async void OnFilterClicked(object sender, EventArgs e)
    {
        _filter = sender == btnAttending ? EventFilter.Attending
                : sender == btnHosting ? EventFilter.Hosting
                : EventFilter.All;
        await LoadEventsAsync();
    }

    private async void OnEventSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not Event selected)
            return;

        cvEvents.SelectedItem = null;
        await Navigation.PushAsync(new EventDetailsPage(selected.Id));
    }

    private async void OnAddEventClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddEventPage());
    }

    private async void OnAccountClicked(object sender, EventArgs e)
    {
        // Log out (or, for a guest, return to the login screen)
        Session.CurrentUser = null;
        await Shell.Current.GoToAsync("//login");
    }
}
