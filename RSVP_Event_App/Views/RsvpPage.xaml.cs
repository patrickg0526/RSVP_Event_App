using RSVP_Event_App.Models;
using RSVP_Event_App.Services;

namespace RSVP_Event_App.Views;

public partial class RsvpPage : ContentPage
{
    private readonly Event _event;

    public RsvpPage(Event selectedEvent)
    {
        InitializeComponent();
        _event = selectedEvent;

        lblEventName.Text = _event.Name;
        lblEventDate.Text = _event.DateDisplay;

        // Logged-in users get their saved details pre-filled; guests start with blank fields
        if (AppData.CurrentUser is { } user)
        {
            txtName.Text = user.Name;
            txtEmail.Text = user.Email;
            txtPhone.Text = user.Phone;
        }
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text) ||
            string.IsNullOrWhiteSpace(txtEmail.Text) ||
            string.IsNullOrWhiteSpace(txtPhone.Text))
        {
            lblError.Text = "Please fill in all fields.";
            lblError.IsVisible = true;
            return;
        }

        // Recording the RSVP will be implemented with data storage in Weeks 3-4
        await DisplayAlert("RSVP Received", $"You're on the list for {_event.Name}!", "OK");
        await Navigation.PopAsync();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
