using RSVP_Event_App.Models;
using RSVP_Event_App.Services;

namespace RSVP_Event_App.Views;

public partial class AddEventPage : ContentPage
{
    public AddEventPage()
    {
        InitializeComponent();

        // Default the host to the logged-in user and the dates to sensible future values
        txtHostName.Text = Session.CurrentUser?.Name ?? string.Empty;
        dpEventDate.MinimumDate = DateTime.Today;
        dpDeadlineDate.MinimumDate = DateTime.Today;
        dpEventDate.Date = DateTime.Today.AddDays(14);
        dpDeadlineDate.Date = DateTime.Today.AddDays(7);
        tpEventTime.Time = new TimeSpan(18, 0, 0);
        tpDeadlineTime.Time = new TimeSpan(23, 59, 0);
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtHostName.Text) ||
            string.IsNullOrWhiteSpace(txtEventName.Text) ||
            string.IsNullOrWhiteSpace(txtAddress.Text) ||
            string.IsNullOrWhiteSpace(txtMaxAttendees.Text))
        {
            ShowError("Please fill in all fields.");
            return;
        }

        if (!int.TryParse(txtMaxAttendees.Text, out int max) || max < 1)
        {
            ShowError("Maximum attendees must be a whole number greater than zero.");
            return;
        }

        DateTime eventDateTime = dpEventDate.Date + tpEventTime.Time;
        DateTime deadline = dpDeadlineDate.Date + tpDeadlineTime.Time;

        if (eventDateTime <= DateTime.Now)
        {
            ShowError("The event date and time must be in the future.");
            return;
        }

        if (deadline > eventDateTime)
        {
            ShowError("The RSVP deadline must be on or before the event date and time.");
            return;
        }

        var ev = new Event
        {
            HostUserId = Session.CurrentUser!.Id,
            HostName = txtHostName.Text.Trim(),
            Name = txtEventName.Text.Trim(),
            Address = txtAddress.Text.Trim(),
            MaxAttendees = max,
            EventDateTime = eventDateTime,
            RsvpDeadline = deadline
        };
        await App.Database.AddEventAsync(ev);

        await DisplayAlert("Event Saved", $"\"{ev.Name}\" has been created.", "OK");
        await Navigation.PopAsync();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void ShowError(string message)
    {
        lblError.Text = message;
        lblError.IsVisible = true;
    }
}
