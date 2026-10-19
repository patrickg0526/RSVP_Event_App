using RSVP_Event_App.Models;
using RSVP_Event_App.Services;

namespace RSVP_Event_App.Views;

public partial class EventDetailsPage : ContentPage
{
    private readonly int _eventId;
    private Event? _event;

    public EventDetailsPage(int eventId)
    {
        InitializeComponent();
        _eventId = eventId;
    }

    // Reload from the database every time the page appears so new RSVPs show up immediately
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _event = await App.Database.GetEventAsync(_eventId);
        if (_event is null)
        {
            await DisplayAlert("Not Found", "This event no longer exists.", "OK");
            await Navigation.PopAsync();
            return;
        }

        lblName.Text = _event.Name;
        lblHost.Text = _event.HostName;
        lblAddress.Text = _event.Address;
        lblDate.Text = _event.DateDisplay;
        lblDeadline.Text = _event.DeadlineDisplay;
        lblMax.Text = _event.MaxAttendees.ToString();
        lblCurrent.Text = _event.CurrentAttendees.ToString();

        var rsvps = await App.Database.GetRsvpsForEventAsync(_eventId);
        stackAttendees.Children.Clear();
        if (rsvps.Count == 0)
        {
            stackAttendees.Children.Add(new Label { Text = "No one has RSVP'd yet." });
        }
        foreach (var rsvp in rsvps)
        {
            stackAttendees.Children.Add(new Label { Text = "• " + rsvp.Name });
        }

        await UpdateRsvpStateAsync();
    }

    private async Task UpdateRsvpStateAsync()
    {
        bool alreadyAttending = Session.CurrentUser is { } user &&
            await App.Database.HasRsvpAsync(_eventId, user.Email);

        string? reason = _event!.IsRsvpClosed ? "RSVP is closed for this event."
                       : _event.IsFull ? "This event is full."
                       : alreadyAttending ? "You're attending this event."
                       : null;

        btnRsvp.IsEnabled = reason is null;
        lblStatus.Text = reason ?? string.Empty;
        lblStatus.IsVisible = reason is not null;
    }

    private async void OnRsvpClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RsvpPage(_event!));
    }
}
