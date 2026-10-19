using SQLite;

namespace RSVP_Event_App.Models;

// One row per person attending an event (links Users and Events, and also supports guests)
[Table("Rsvps")]
public class Rsvp
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Foreign key to Events.Id. EventId + Email is unique so nobody can RSVP twice
    [Indexed(Name = "UX_Rsvp_Event_Email", Order = 1, Unique = true), NotNull]
    public int EventId { get; set; }

    // Foreign key to Users.Id; null when the RSVP was made by a guest
    [Indexed]
    public int? UserId { get; set; }

    [NotNull, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Indexed(Name = "UX_Rsvp_Event_Email", Order = 2, Unique = true), NotNull, MaxLength(254), Collation("NOCASE")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    public DateTime RsvpDate { get; set; } = DateTime.Now;
}
