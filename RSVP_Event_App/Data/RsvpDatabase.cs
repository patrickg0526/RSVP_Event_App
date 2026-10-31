using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using RSVP_Event_App.Models;
using RSVP_Event_App.Services;

namespace RSVP_Event_App.Data;

public record RsvpResult(bool Success, string Message);

// Thrown when the RSVP web service can't be reached, so pages can show a friendly message
public class ServiceUnavailableException(Exception inner)
    : Exception("The RSVP service could not be reached. Make sure the RsvpApi web service is running.", inner);

// Data Access Layer. In Week 3 this class read and wrote a local SQLite database;
// it now calls the RsvpApi web service instead, keeping the same methods so the pages barely changed.
public class RsvpDatabase
{
    // Must match the applicationUrl in RsvpApi/Properties/launchSettings.json
    public const string BaseAddress = "http://localhost:5090/";

    private static readonly HttpClient Client = new() { BaseAddress = new Uri(BaseAddress), Timeout = TimeSpan.FromSeconds(15) };

    // ---------- Users ----------

    public async Task<(User? User, string Message)> AddUserAsync(string name, string email, string password, string phone)
    {
        var response = await SendAsync(HttpMethod.Post, "api/Users", new { name, email, password, phone });
        return response.IsSuccessStatusCode
            ? (await ReadAsync<User>(response), string.Empty)
            : (null, await ReadErrorAsync(response));
    }

    // Login: the web service validates the credentials using Basic Authentication
    public async Task<User?> ValidateCredentialsAsync(string email, string password)
    {
        var response = await SendAsync(HttpMethod.Get, "api/Users/me", auth: Session.BasicHeader(email.Trim(), password));
        return response.IsSuccessStatusCode ? await ReadAsync<User>(response) : null;
    }

    // ---------- Events ----------

    public Task<List<Event>> GetAllEventsAsync() => GetListAsync<Event>("api/Events");

    public Task<List<Event>> GetHostingEventsAsync(int userId) => GetListAsync<Event>("api/Events/hosting", Session.AuthorizationHeader);

    public Task<List<Event>> GetAttendingEventsAsync(User user) => GetListAsync<Event>("api/Events/attending", Session.AuthorizationHeader);

    public async Task<Event?> GetEventAsync(int id)
    {
        var response = await SendAsync(HttpMethod.Get, $"api/Events/{id}");
        return response.IsSuccessStatusCode ? await ReadAsync<Event>(response) : null;
    }

    public async Task<RsvpResult> AddEventAsync(Event ev)
    {
        var response = await SendAsync(HttpMethod.Post, "api/Events", new
        {
            ev.HostName,
            ev.Name,
            ev.Address,
            ev.MaxAttendees,
            ev.EventDateTime,
            ev.RsvpDeadline
        }, Session.AuthorizationHeader);

        return response.IsSuccessStatusCode
            ? new RsvpResult(true, $"\"{ev.Name}\" has been created.")
            : new RsvpResult(false, await ReadErrorAsync(response));
    }

    // ---------- RSVPs ----------

    public Task<List<Rsvp>> GetRsvpsForEventAsync(int eventId) => GetListAsync<Rsvp>($"api/Events/{eventId}/attendees");

    // Whether the logged-in user has already RSVP'd (guests are checked when they submit)
    public async Task<bool> HasRsvpAsync(int eventId, string email)
    {
        if (Session.AuthorizationHeader is null)
            return false;

        var response = await SendAsync(HttpMethod.Get, $"api/Events/{eventId}/rsvp", auth: Session.AuthorizationHeader);
        if (!response.IsSuccessStatusCode)
            return false;
        var status = await ReadAsync<RsvpStatus>(response);
        return status?.Attending ?? false;
    }

    // Logged-in users RSVP with their saved credentials; guests send their name, e-mail, and phone.
    // The web service enforces the deadline, capacity, and duplicate-RSVP rules.
    public async Task<RsvpResult> AddRsvpAsync(Rsvp rsvp)
    {
        var response = await SendAsync(HttpMethod.Post, $"api/Events/{rsvp.EventId}/rsvps",
            new { rsvp.Name, rsvp.Email, rsvp.Phone }, Session.AuthorizationHeader);

        return response.IsSuccessStatusCode
            ? new RsvpResult(true, "You're on the list!")
            : new RsvpResult(false, await ReadErrorAsync(response));
    }

    // ---------- HTTP helpers ----------

    private record RsvpStatus(bool Attending);

    private static async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body = null,
        AuthenticationHeaderValue? auth = null)
    {
        using var request = new HttpRequestMessage(method, url);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (auth is not null)
            request.Headers.Authorization = auth;
        if (body is not null)
            request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

        try
        {
            return await Client.SendAsync(request);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new ServiceUnavailableException(ex);
        }
    }

    private static async Task<List<T>> GetListAsync<T>(string url, AuthenticationHeaderValue? auth = null)
    {
        var response = await SendAsync(HttpMethod.Get, url, auth: auth);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return new List<T>();
        response.EnsureSuccessStatusCode();
        return await ReadAsync<List<T>>(response) ?? new List<T>();
    }

    private static async Task<T?> ReadAsync<T>(HttpResponseMessage response) =>
        JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());

    // Error bodies are either a JSON-encoded string or plain text
    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        string body = await response.Content.ReadAsStringAsync();
        if (body.StartsWith('"'))
        {
            try { body = JsonConvert.DeserializeObject<string>(body) ?? string.Empty; }
            catch (JsonException) { }
        }
        return string.IsNullOrWhiteSpace(body) || body.StartsWith('{')
            ? $"The request failed ({(int)response.StatusCode} {response.ReasonPhrase})."
            : body;
    }
}
