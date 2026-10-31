using SQLite;

namespace RsvpApi.Models;

// Database tables (stored server-side in SQLite). These are never returned directly;
// the controllers return the DTOs in Dtos.cs so password hashes stay on the server.

[Table("Users")]
public class User
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Unique, NotNull, MaxLength(254), Collation("NOCASE")]
    public string Email { get; set; } = string.Empty;

    [NotNull]
    public string PasswordHash { get; set; } = string.Empty;

    [NotNull]
    public string PasswordSalt { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

[Table("Events")]
public class Event
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed, NotNull]
    public int HostUserId { get; set; }

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
}

[Table("Rsvps")]
public class Rsvp
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed(Name = "UX_Rsvp_Event_Email", Order = 1, Unique = true), NotNull]
    public int EventId { get; set; }

    // Null when the RSVP was made by a guest
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
