namespace RSVP_Event_App.Models;

// Account information returned by the RSVP web service (passwords never leave the server)
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
