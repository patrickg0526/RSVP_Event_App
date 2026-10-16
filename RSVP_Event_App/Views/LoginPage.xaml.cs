using RSVP_Event_App.Services;

namespace RSVP_Event_App.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string email = txtEmail.Text?.Trim() ?? string.Empty;
        string password = txtPassword.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ShowError("Please enter your e-mail address and password.");
            return;
        }

        // Hard-coded credentials for Week 2; web service authentication comes in Week 4
        if (!string.Equals(email, AppData.DemoUser.Email, StringComparison.OrdinalIgnoreCase) ||
            password != AppData.DemoUser.Password)
        {
            ShowError("Invalid e-mail address or password.");
            return;
        }

        AppData.CurrentUser = AppData.DemoUser;
        ClearForm();
        await Shell.Current.GoToAsync("//events");
    }

    private async void OnGuestClicked(object sender, EventArgs e)
    {
        AppData.CurrentUser = null;
        ClearForm();
        await Shell.Current.GoToAsync("//events");
    }

    private async void OnCreateAccountClicked(object sender, EventArgs e)
    {
        lblError.IsVisible = false;
        await Navigation.PushAsync(new CreateAccountPage());
    }

    private void ShowError(string message)
    {
        lblError.Text = message;
        lblError.IsVisible = true;
    }

    private void ClearForm()
    {
        txtEmail.Text = string.Empty;
        txtPassword.Text = string.Empty;
        lblError.IsVisible = false;
    }
}
