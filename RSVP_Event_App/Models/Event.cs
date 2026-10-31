namespace RSVP_Event_App.Models;

// Event as returned by the RSVP web service
public class Event
{
    public int Id { get; set; }
    public int HostUserId { get; set; }
    public string HostName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int MaxAttendees { get; set; }
    public DateTime EventDateTime { get; set; }
    public DateTime RsvpDeadline { get; set; }

    // Calculated by the web service from the RSVPs table
    public int CurrentAttendees { get; set; }

    public bool IsFull => CurrentAttendees >= MaxAttendees;
    public bool IsRsvpClosed => DateTime.Now > RsvpDeadline;
    public string DateDisplay => EventDateTime.ToString("ddd, MMM d, yyyy 'at' h:mm tt");
    public string DeadlineDisplay => RsvpDeadline.ToString("ddd, MMM d, yyyy 'at' h:mm tt");
    public string AttendanceDisplay => $"{CurrentAttendees} / {MaxAttendees} attending";
}
