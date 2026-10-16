namespace RSVP_Event_App.Models;

public class Event
{
    public int Id { get; set; }
    public string HostName { get; set; } = string.Empty;
    public string HostEmail { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int MaxAttendees { get; set; }
    public DateTime EventDateTime { get; set; }
    public DateTime RsvpDeadline { get; set; }
    public List<string> AttendeeNames { get; set; } = new();
    public List<string> AttendeeEmails { get; set; } = new();

    public int CurrentAttendees => AttendeeNames.Count;
    public bool IsFull => CurrentAttendees >= MaxAttendees;
    public bool IsRsvpClosed => DateTime.Now > RsvpDeadline;

    // Display helpers used by the event list and details pages
    public string DateDisplay => EventDateTime.ToString("ddd, MMM d, yyyy 'at' h:mm tt");
    public string DeadlineDisplay => RsvpDeadline.ToString("ddd, MMM d, yyyy 'at' h:mm tt");
    public string AttendanceDisplay => $"{CurrentAttendees} / {MaxAttendees} attending";
}
