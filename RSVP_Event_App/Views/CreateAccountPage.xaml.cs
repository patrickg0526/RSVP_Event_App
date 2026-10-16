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

        if (!txtEmail.Text.Contains('@') || !txtEmail.Text.Contains('.'))
        {
            ShowError("Please enter a valid e-mail address.");
            return;
        }

        if (txtPassword.Text != txtConfirmPassword.Text)
        {
            ShowError("Passwords do not match.");
            return;
        }

        // Saving the account will be implemented with local storage in Week 3
        await DisplayAlert("Account Created",
            $"Welcome, {txtName.Text.Trim()}! You can now log in.", "OK");
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
