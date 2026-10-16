using RSVP_Event_App.Models;

namespace RSVP_Event_App.Services;

// Hard-coded data and session state for the Week 2 UI/navigation milestone.
// Saving users, events, and RSVPs will be replaced with real storage in Week 3.
public static class AppData
{
    // Hard-coded credentials for the validated-user login
    public static readonly User DemoUser = new()
    {
        Name = "Patrick Gonzalez",
        Email = "patrick@rsvpapp.com",
        Password = "Password1",
        Phone = "555-123-4567"
    };

    // The logged-in user, or null when browsing as a guest
    public static User? CurrentUser { get; set; }

    public static bool IsGuest => CurrentUser is null;

    public static List<Event> Events { get; } = new()
    {
        new Event
        {
            Id = 1, HostName = "Patrick Gonzalez", HostEmail = "patrick@rsvpapp.com",
            Name = "Fall Game Night", Address = "214 Maple Ave, Greenville, SC",
            MaxAttendees = 12, EventDateTime = new DateTime(2026, 10, 31, 19, 0, 0),
            RsvpDeadline = new DateTime(2026, 10, 28, 23, 59, 0),
            AttendeeNames = new() { "Maria Lopez", "James Carter", "Aisha Brown", "Kevin Nguyen" },
            AttendeeEmails = new() { "maria@example.com", "james@example.com", "aisha@example.com", "kevin@example.com" }
        },
        new Event
        {
            Id = 2, HostName = "Sarah Mitchell", HostEmail = "sarah@example.com",
            Name = "Friendsgiving Potluck", Address = "88 Pine St, Greenville, SC",
            MaxAttendees = 20, EventDateTime = new DateTime(2026, 11, 21, 17, 30, 0),
            RsvpDeadline = new DateTime(2026, 11, 14, 23, 59, 0),
            AttendeeNames = new() { "Patrick Gonzalez", "Daniel Kim", "Emily Rivera" },
            AttendeeEmails = new() { "patrick@rsvpapp.com", "daniel@example.com", "emily@example.com" }
        },
        new Event
        {
            Id = 3, HostName = "Marcus Johnson", HostEmail = "marcus@example.com",
            Name = "Charity 5K Fun Run", Address = "Cleveland Park, Greenville, SC",
            MaxAttendees = 50, EventDateTime = new DateTime(2026, 11, 7, 8, 0, 0),
            RsvpDeadline = new DateTime(2026, 11, 1, 23, 59, 0),
            AttendeeNames = new() { "Patrick Gonzalez", "Olivia Chen", "Noah Patel", "Grace Wilson", "Ethan Moore" },
            AttendeeEmails = new() { "patrick@rsvpapp.com", "olivia@example.com", "noah@example.com", "grace@example.com", "ethan@example.com" }
        },
        new Event
        {
            Id = 4, HostName = "Patrick Gonzalez", HostEmail = "patrick@rsvpapp.com",
            Name = "Coding Study Group", Address = "ECPI University, Greenville, SC",
            MaxAttendees = 8, EventDateTime = new DateTime(2026, 10, 24, 18, 0, 0),
            RsvpDeadline = new DateTime(2026, 10, 23, 12, 0, 0),
            AttendeeNames = new() { "Tyler Brooks", "Hannah Scott" },
            AttendeeEmails = new() { "tyler@example.com", "hannah@example.com" }
        },
        new Event
        {
            Id = 5, HostName = "Linda Torres", HostEmail = "linda@example.com",
            Name = "Holiday Cookie Swap", Address = "502 Oak Ridge Dr, Simpsonville, SC",
            MaxAttendees = 6, EventDateTime = new DateTime(2026, 12, 12, 14, 0, 0),
            RsvpDeadline = new DateTime(2026, 12, 5, 23, 59, 0),
            AttendeeNames = new() { "Rosa Diaz", "Mark Evans", "Jenna Lee", "Chris Adams", "Paula Gray", "Sam Fox" },
            AttendeeEmails = new() { "rosa@example.com", "mark@example.com", "jenna@example.com", "chris@example.com", "paula@example.com", "sam@example.com" }
        }
    };

    public static IEnumerable<Event> AttendingEvents(User user) =>
        Events.Where(e => e.AttendeeEmails.Contains(user.Email, StringComparer.OrdinalIgnoreCase));

    public static IEnumerable<Event> HostingEvents(User user) =>
        Events.Where(e => string.Equals(e.HostEmail, user.Email, StringComparison.OrdinalIgnoreCase));
}
