using System.Windows.Input;
using RSVP_Event_App.Data;
using RSVP_Event_App.Models;
using RSVP_Event_App.Services;

namespace RSVP_Event_App.ViewModels;

public class RsvpViewModel : BaseViewModel, IQueryAttributable
{
    public const string EventKey = "Event";

    private readonly RsvpDatabase _database;
    private Event? _event;

    public RsvpViewModel(RsvpDatabase database)
    {
        _database = database;
        SubmitCommand = new Command(async () => await SubmitAsync());
        CancelCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue(EventKey, out var value))
            return;

        _event = (Event)value;
        EventName = _event.Name;
        EventDate = _event.DateDisplay;

        // Logged-in users RSVP with their saved account details; guests start with blank fields
        if (Session.CurrentUser is { } user)
        {
            IsLoggedIn = true;
            Name = user.Name;
            Email = user.Email;
            Phone = user.Phone;
        }
    }

    private string _eventName = string.Empty;
    public string EventName { get => _eventName; set => SetProperty(ref _eventName, value); }

    private string _eventDate = string.Empty;
    public string EventDate { get => _eventDate; set => SetProperty(ref _eventDate, value); }

    private bool _isLoggedIn;
    public bool IsLoggedIn { get => _isLoggedIn; set => SetProperty(ref _isLoggedIn, value); }

    private string _name = string.Empty;
    public string Name { get => _name; set => SetProperty(ref _name, value); }

    private string _email = string.Empty;
    public string Email { get => _email; set => SetProperty(ref _email, value); }

    private string _phone = string.Empty;
    public string Phone { get => _phone; set => SetProperty(ref _phone, value); }

    public ICommand SubmitCommand { get; }
    public ICommand CancelCommand { get; }

    private async Task SubmitAsync()
    {
        if (IsBusy || _event is null)
            return;

        ClearError();

        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Phone))
        {
            ErrorMessage = "Please fill in all fields.";
            return;
        }

        if (!Email.Contains('@') || !Email.Contains('.'))
        {
            ErrorMessage = "Please enter a valid e-mail address.";
            return;
        }

        try
        {
            IsBusy = true;

            RsvpResult result = await _database.AddRsvpAsync(new Rsvp
            {
                EventId = _event.Id,
                UserId = Session.CurrentUser?.Id,
                Name = Name.Trim(),
                Email = Email.Trim(),
                Phone = Phone.Trim()
            });

            if (!result.Success)
            {
                ErrorMessage = result.Message;
                return;
            }

            await Shell.Current.DisplayAlert("RSVP Received", result.Message, "OK");
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
