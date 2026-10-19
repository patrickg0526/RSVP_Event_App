using SQLite;

namespace RSVP_Event_App.Models;

[Table("Users")]
public class User
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    // Used as the login name, so it must be unique
    [Unique, NotNull, MaxLength(254), Collation("NOCASE")]
    public string Email { get; set; } = string.Empty;

    // Passwords are never stored in plain text: PBKDF2 hash + per-user random salt (Base64)
    [NotNull]
    public string PasswordHash { get; set; } = string.Empty;

    [NotNull]
    public string PasswordSalt { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
