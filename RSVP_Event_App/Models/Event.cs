using SQLite;

namespace RSVP_Event_App.Models;

[Table("Events")]
public class Event
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Foreign key to Users.Id (the account that created the event)
    [Indexed, NotNull]
    public int HostUserId { get; set; }

    // Display name of the person hosting (may differ from the account name)
    [NotNull, MaxLength(100)]
    public string HostName { get; set; } = string.Empty;

    [NotNull, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [NotNull, MaxLength(200)]
    public string Address { get; set; } = string.Empty;

    public int MaxAttendees { get; set; }

    public DateTime EventDateTime { get; set; }

    public DateTime RsvpDeadline { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Not stored: filled in by the data access layer from the Rsvps table
    [Ignore]
    public int CurrentAttendees { get; set; }

    [Ignore] public bool IsFull => CurrentAttendees >= MaxAttendees;
    [Ignore] public bool IsRsvpClosed => DateTime.Now > RsvpDeadline;
    [Ignore] public string DateDisplay => EventDateTime.ToString("ddd, MMM d, yyyy 'at' h:mm tt");
    [Ignore] public string DeadlineDisplay => RsvpDeadline.ToString("ddd, MMM d, yyyy 'at' h:mm tt");
    [Ignore] public string AttendanceDisplay => $"{CurrentAttendees} / {MaxAttendees} attending";
}
