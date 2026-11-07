using System.Windows.Input;
using RSVP_Event_App.Data;
using RSVP_Event_App.Models;
using RSVP_Event_App.Services;
using RSVP_Event_App.Views;

namespace RSVP_Event_App.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly RsvpDatabase _database;

    public LoginViewModel(RsvpDatabase database)
    {
        _database = database;
        LoginCommand = new Command(async () => await LoginAsync());
        GuestCommand = new Command(async () => await ContinueAsGuestAsync());
        CreateAccountCommand = new Command(async () => await OpenCreateAccountAsync());
    }

    private string _email = string.Empty;
    public string Email { get => _email; set => SetProperty(ref _email, value); }

    private string _password = string.Empty;
    public string Password { get => _password; set => SetProperty(ref _password, value); }

    public ICommand LoginCommand { get; }
    public ICommand GuestCommand { get; }
    public ICommand CreateAccountCommand { get; }

    private async Task LoginAsync()
    {
        if (IsBusy)
            return;

        ClearError();
        string email = Email.Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter your e-mail address and password.";
            return;
        }

        try
        {
            IsBusy = true;

            // Credentials are verified by the RSVP web service using Basic Authentication
            User? user = await _database.ValidateCredentialsAsync(email, Password);
            if (user is null)
            {
                ErrorMessage = "Invalid e-mail address or password.";
                return;
            }

            Session.SignIn(user, Password);
            ClearForm();
            await Shell.Current.GoToAsync("//events");
        }
        catch (ServiceUnavailableException ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ContinueAsGuestAsync()
    {
        Session.SignOut();
        ClearForm();
        await Shell.Current.GoToAsync("//events");
    }

    private async Task OpenCreateAccountAsync()
    {
        ClearError();
        await Shell.Current.GoToAsync(nameof(CreateAccountPage));
    }

    private void ClearForm()
    {
        Email = string.Empty;
        Password = string.Empty;
        ClearError();
    }
}
