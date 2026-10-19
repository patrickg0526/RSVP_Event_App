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

        // Logged-in users get their saved account details pre-filled; guests start with blank fields
        if (Session.CurrentUser is { } user)
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
            ShowError("Please fill in all fields.");
            return;
        }

        if (!txtEmail.Text.Contains('@') || !txtEmail.Text.Contains('.'))
        {
            ShowError("Please enter a valid e-mail address.");
            return;
        }

        var result = await App.Database.AddRsvpAsync(new Rsvp
        {
            EventId = _event.Id,
            UserId = Session.CurrentUser?.Id,
            Name = txtName.Text.Trim(),
            Email = txtEmail.Text.Trim(),
            Phone = txtPhone.Text.Trim()
        });

        if (!result.Success)
        {
            ShowError(result.Message);
            return;
        }

        await DisplayAlert("RSVP Received", result.Message, "OK");
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
