using System.Windows.Input;
using RSVP_Event_App.Data;

namespace RSVP_Event_App.ViewModels;

public class CreateAccountViewModel : BaseViewModel
{
    private readonly RsvpDatabase _database;

    public CreateAccountViewModel(RsvpDatabase database)
    {
        _database = database;
        CreateCommand = new Command(async () => await CreateAsync());
        CancelCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    private string _name = string.Empty;
    public string Name { get => _name; set => SetProperty(ref _name, value); }

    private string _email = string.Empty;
    public string Email { get => _email; set => SetProperty(ref _email, value); }

    private string _password = string.Empty;
    public string Password { get => _password; set => SetProperty(ref _password, value); }

    private string _confirmPassword = string.Empty;
    public string ConfirmPassword { get => _confirmPassword; set => SetProperty(ref _confirmPassword, value); }

    private string _phone = string.Empty;
    public string Phone { get => _phone; set => SetProperty(ref _phone, value); }

    public ICommand CreateCommand { get; }
    public ICommand CancelCommand { get; }

    private async Task CreateAsync()
    {
        if (IsBusy)
            return;

        ClearError();

        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword) ||
            string.IsNullOrWhiteSpace(Phone))
        {
            ErrorMessage = "Please fill in all fields.";
            return;
        }

        string email = Email.Trim();
        if (!email.Contains('@') || !email.Contains('.'))
        {
            ErrorMessage = "Please enter a valid e-mail address.";
            return;
        }

        if (Password.Length < 8)
        {
            ErrorMessage = "Password must be at least 8 characters.";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return;
        }

        try
        {
            IsBusy = true;

            // The web service saves the account and rejects duplicate e-mail addresses
            var (user, message) = await _database.AddUserAsync(Name, email, Password, Phone);
            if (user is null)
            {
                ErrorMessage = message;
                return;
            }

            await Shell.Current.DisplayAlert("Account Created", $"Welcome, {user.Name}! You can now log in.", "OK");
            await Shell.Current.GoToAsync("..");
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
}
