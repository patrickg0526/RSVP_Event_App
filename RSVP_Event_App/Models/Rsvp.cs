namespace RSVP_Event_App.Models;

// An RSVP sent to the web service, or an attendee returned from it (only Name and RsvpDate are returned)
public class Rsvp
{
    public int EventId { get; set; }
    public int? UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime RsvpDate { get; set; }
}
