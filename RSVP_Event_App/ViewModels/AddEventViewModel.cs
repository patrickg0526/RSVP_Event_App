using System.Windows.Input;
using RSVP_Event_App.Data;
using RSVP_Event_App.Models;
using RSVP_Event_App.Services;

namespace RSVP_Event_App.ViewModels;

public class AddEventViewModel : BaseViewModel
{
    private readonly RsvpDatabase _database;

    public AddEventViewModel(RsvpDatabase database)
    {
        _database = database;
        SaveCommand = new Command(async () => await SaveAsync());
        CancelCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

        // Default the host to the logged-in user and the dates to sensible future values
        _hostName = Session.CurrentUser?.Name ?? string.Empty;
    }

    public DateTime MinimumDate { get; } = DateTime.Today;

    private string _hostName;
    public string HostName { get => _hostName; set => SetProperty(ref _hostName, value); }

    private string _eventName = string.Empty;
    public string EventName { get => _eventName; set => SetProperty(ref _eventName, value); }

    private string _address = string.Empty;
    public string Address { get => _address; set => SetProperty(ref _address, value); }

    private string _maxAttendees = string.Empty;
    public string MaxAttendees { get => _maxAttendees; set => SetProperty(ref _maxAttendees, value); }

    private DateTime _eventDate = DateTime.Today.AddDays(14);
    public DateTime EventDate { get => _eventDate; set => SetProperty(ref _eventDate, value); }

    private TimeSpan _eventTime = new(18, 0, 0);
    public TimeSpan EventTime { get => _eventTime; set => SetProperty(ref _eventTime, value); }

    private DateTime _deadlineDate = DateTime.Today.AddDays(7);
    public DateTime DeadlineDate { get => _deadlineDate; set => SetProperty(ref _deadlineDate, value); }

    private TimeSpan _deadlineTime = new(23, 59, 0);
    public TimeSpan DeadlineTime { get => _deadlineTime; set => SetProperty(ref _deadlineTime, value); }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    private async Task SaveAsync()
    {
        if (IsBusy)
            return;

        ClearError();

        if (string.IsNullOrWhiteSpace(HostName) || string.IsNullOrWhiteSpace(EventName) ||
            string.IsNullOrWhiteSpace(Address) || string.IsNullOrWhiteSpace(MaxAttendees))
        {
            ErrorMessage = "Please fill in all fields.";
            return;
        }

        if (!int.TryParse(MaxAttendees, out int max) || max < 1)
        {
            ErrorMessage = "Maximum attendees must be a whole number greater than zero.";
            return;
        }

        DateTime eventDateTime = EventDate + EventTime;
        DateTime deadline = DeadlineDate + DeadlineTime;

        if (eventDateTime <= DateTime.Now)
        {
            ErrorMessage = "The event date and time must be in the future.";
            return;
        }

        if (deadline <= DateTime.Now)
        {
            ErrorMessage = "The RSVP deadline must be in the future.";
            return;
        }

        if (deadline > eventDateTime)
        {
            ErrorMessage = "The RSVP deadline must be on or before the event date and time.";
            return;
        }

        try
        {
            IsBusy = true;

            RsvpResult result = await _database.AddEventAsync(new Event
            {
                HostUserId = Session.CurrentUser!.Id,
                HostName = HostName.Trim(),
                Name = EventName.Trim(),
                Address = Address.Trim(),
                MaxAttendees = max,
                EventDateTime = eventDateTime,
                RsvpDeadline = deadline
            });

            if (!result.Success)
            {
                ErrorMessage = result.Message;
                return;
            }

            await Shell.Current.DisplayAlert("Event Saved", result.Message, "OK");
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
