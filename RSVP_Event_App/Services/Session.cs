using RSVP_Event_App.Models;

namespace RSVP_Event_App.Services;

// Tracks who is using the app for the current run
public static class Session
{
    // The logged-in user, or null when browsing as a guest
    public static User? CurrentUser { get; set; }

    public static bool IsGuest => CurrentUser is null;
}
