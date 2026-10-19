namespace RSVP_Event_App.Views;

public partial class CreateAccountPage : ContentPage
{
    public CreateAccountPage()
    {
        InitializeComponent();
    }

    private async void OnCreateClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text) ||
            string.IsNullOrWhiteSpace(txtEmail.Text) ||
            string.IsNullOrWhiteSpace(txtPassword.Text) ||
            string.IsNullOrWhiteSpace(txtConfirmPassword.Text) ||
            string.IsNullOrWhiteSpace(txtPhone.Text))
        {
            ShowError("Please fill in all fields.");
            return;
        }

        string email = txtEmail.Text.Trim();
        if (!email.Contains('@') || !email.Contains('.'))
        {
            ShowError("Please enter a valid e-mail address.");
            return;
        }

        if (txtPassword.Text.Length < 8)
        {
            ShowError("Password must be at least 8 characters.");
            return;
        }

        if (txtPassword.Text != txtConfirmPassword.Text)
        {
            ShowError("Passwords do not match.");
            return;
        }

        if (await App.Database.GetUserByEmailAsync(email) is not null)
        {
            ShowError("An account with this e-mail address already exists.");
            return;
        }

        var user = await App.Database.AddUserAsync(txtName.Text, email, txtPassword.Text, txtPhone.Text);
        await DisplayAlert("Account Created", $"Welcome, {user.Name}! You can now log in.", "OK");
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
