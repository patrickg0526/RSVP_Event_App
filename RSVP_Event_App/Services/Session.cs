using System.Net.Http.Headers;
using System.Text;
using RSVP_Event_App.Models;

namespace RSVP_Event_App.Services;

// Tracks who is using the app for the current run
public static class Session
{
    // The logged-in user's credentials are kept in memory only (never written to disk)
    // and re-sent to the web service with Basic Authentication for user-specific requests.
    private static string? _password;

    // The logged-in user, or null when browsing as a guest
    public static User? CurrentUser { get; private set; }

    public static bool IsGuest => CurrentUser is null;

    // Changes on every log in and log out, so pages can tell that a new session has started
    public static int Version { get; private set; }

    public static void SignIn(User user, string password)
    {
        CurrentUser = user;
        _password = password;
        Version++;
    }

    public static void SignOut()
    {
        CurrentUser = null;
        _password = null;
        Version++;
    }

    public static AuthenticationHeaderValue? AuthorizationHeader =>
        CurrentUser is null || _password is null
            ? null
            : BasicHeader(CurrentUser.Email, _password);

    public static AuthenticationHeaderValue BasicHeader(string email, string password) =>
        new("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{email}:{password}")));
}
