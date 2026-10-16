using RSVP_Event_App.Models;
using RSVP_Event_App.Services;

namespace RSVP_Event_App.Views;

public partial class EventDetailsPage : ContentPage
{
    private readonly Event _event;

    public EventDetailsPage(Event selectedEvent)
    {
        InitializeComponent();
        _event = selectedEvent;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        lblName.Text = _event.Name;
        lblHost.Text = _event.HostName;
        lblAddress.Text = _event.Address;
        lblDate.Text = _event.DateDisplay;
        lblDeadline.Text = _event.DeadlineDisplay;
        lblMax.Text = _event.MaxAttendees.ToString();
        lblCurrent.Text = _event.CurrentAttendees.ToString();

        stackAttendees.Children.Clear();
        if (_event.AttendeeNames.Count == 0)
        {
            stackAttendees.Children.Add(new Label { Text = "No one has RSVP'd yet." });
        }
        foreach (string name in _event.AttendeeNames)
        {
            stackAttendees.Children.Add(new Label { Text = "• " + name });
        }

        UpdateRsvpState();
    }

    private void UpdateRsvpState()
    {
        bool alreadyAttending = AppData.CurrentUser is { } user &&
            _event.AttendeeEmails.Contains(user.Email, StringComparer.OrdinalIgnoreCase);

        string? reason = _event.IsRsvpClosed ? "RSVP is closed for this event."
                       : _event.IsFull ? "This event is full."
                       : alreadyAttending ? "You're attending this event."
                       : null;

        btnRsvp.IsEnabled = reason is null;
        lblStatus.Text = reason ?? string.Empty;
        lblStatus.IsVisible = reason is not null;
    }

    private async void OnRsvpClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RsvpPage(_event));
    }
}
