using SQLite;
using RSVP_Event_App.Models;
using RSVP_Event_App.Services;

namespace RSVP_Event_App.Data;

public record RsvpResult(bool Success, string Message);

// Data Access Layer: the only class that talks to the SQLite database.
// Pages call these methods and never write SQL or open connections themselves.
public class RsvpDatabase
{
    private SQLiteAsyncConnection? _database;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    private async Task Init()
    {
        if (_database is not null)
            return;

        await _initLock.WaitAsync();
        try
        {
            if (_database is not null)
                return;

            var db = new SQLiteAsyncConnection(DatabaseConstants.DatabasePath, DatabaseConstants.Flags);
            await db.CreateTableAsync<User>();
            await db.CreateTableAsync<Event>();
            await db.CreateTableAsync<Rsvp>();
            _database = db;

            if (await _database.Table<User>().CountAsync() == 0)
                await SeedAsync();
        }
        finally
        {
            _initLock.Release();
        }
    }

    // ---------- Users ----------

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        await Init();
        string normalized = email.Trim();
        return await _database!.Table<User>().Where(u => u.Email == normalized).FirstOrDefaultAsync();
    }

    public async Task<User> AddUserAsync(string name, string email, string password, string phone)
    {
        await Init();
        var (hash, salt) = PasswordHasher.HashPassword(password);
        var user = new User
        {
            Name = name.Trim(),
            Email = email.Trim(),
            PasswordHash = hash,
            PasswordSalt = salt,
            Phone = phone.Trim()
        };
        await _database!.InsertAsync(user);
        return user;
    }

    // Returns the user if the e-mail exists and the password matches the stored hash
    public async Task<User?> ValidateCredentialsAsync(string email, string password)
    {
        var user = await GetUserByEmailAsync(email);
        if (user is null)
            return null;
        return PasswordHasher.Verify(password, user.PasswordHash, user.PasswordSalt) ? user : null;
    }

    // ---------- Events ----------

    public async Task<List<Event>> GetAllEventsAsync()
    {
        await Init();
        var events = await _database!.Table<Event>().OrderBy(e => e.EventDateTime).ToListAsync();
        return await WithAttendeeCounts(events);
    }

    public async Task<List<Event>> GetHostingEventsAsync(int userId)
    {
        await Init();
        var events = await _database!.Table<Event>()
            .Where(e => e.HostUserId == userId)
            .OrderBy(e => e.EventDateTime)
            .ToListAsync();
        return await WithAttendeeCounts(events);
    }

    public async Task<List<Event>> GetAttendingEventsAsync(User user)
    {
        await Init();
        var events = await _database!.QueryAsync<Event>(
            "SELECT e.* FROM Events e JOIN Rsvps r ON r.EventId = e.Id " +
            "WHERE r.UserId = ? OR r.Email = ? COLLATE NOCASE ORDER BY e.EventDateTime",
            user.Id, user.Email);
        return await WithAttendeeCounts(events);
    }

    public async Task<Event?> GetEventAsync(int id)
    {
        await Init();
        var ev = await _database!.Table<Event>().Where(e => e.Id == id).FirstOrDefaultAsync();
        if (ev is not null)
            ev.CurrentAttendees = await _database.Table<Rsvp>().Where(r => r.EventId == id).CountAsync();
        return ev;
    }

    public async Task<int> AddEventAsync(Event ev)
    {
        await Init();
        ev.CreatedAt = DateTime.Now;
        return await _database!.InsertAsync(ev);
    }

    private async Task<List<Event>> WithAttendeeCounts(List<Event> events)
    {
        var counts = await _database!.QueryAsync<EventCount>(
            "SELECT EventId, COUNT(*) AS Total FROM Rsvps GROUP BY EventId");
        var lookup = counts.ToDictionary(c => c.EventId, c => c.Total);
        foreach (var ev in events)
            ev.CurrentAttendees = lookup.GetValueOrDefault(ev.Id);
        return events;
    }

    private class EventCount
    {
        public int EventId { get; set; }
        public int Total { get; set; }
    }

    // ---------- RSVPs ----------

    public async Task<List<Rsvp>> GetRsvpsForEventAsync(int eventId)
    {
        await Init();
        return await _database!.Table<Rsvp>()
            .Where(r => r.EventId == eventId)
            .OrderBy(r => r.RsvpDate)
            .ToListAsync();
    }

    public async Task<bool> HasRsvpAsync(int eventId, string email)
    {
        await Init();
        string normalized = email.Trim();
        return await _database!.Table<Rsvp>()
            .Where(r => r.EventId == eventId && r.Email == normalized)
            .CountAsync() > 0;
    }

    // Enforces the project rules: before the deadline, not full, and no duplicate RSVPs
    public async Task<RsvpResult> AddRsvpAsync(Rsvp rsvp)
    {
        var ev = await GetEventAsync(rsvp.EventId);
        if (ev is null)
            return new(false, "This event no longer exists.");
        if (ev.IsRsvpClosed)
            return new(false, "The RSVP deadline for this event has passed.");
        if (ev.IsFull)
            return new(false, "Sorry, this event is full.");
        if (await HasRsvpAsync(rsvp.EventId, rsvp.Email))
            return new(false, "An RSVP for this e-mail address already exists for this event.");

        rsvp.RsvpDate = DateTime.Now;
        await _database!.InsertAsync(rsvp);
        return new(true, $"You're on the list for {ev.Name}!");
    }

    // ---------- Sample data (first run only) ----------

    private async Task SeedAsync()
    {
        var patrick = await AddUserAsync("Patrick Gonzalez", "patrick@rsvpapp.com", "Password1", "555-123-4567");
        var sarah = await AddUserAsync("Sarah Mitchell", "sarah@example.com", "Password1", "555-201-3344");
        var marcus = await AddUserAsync("Marcus Johnson", "marcus@example.com", "Password1", "555-301-7788");
        var linda = await AddUserAsync("Linda Torres", "linda@example.com", "Password1", "555-401-9900");

        int year = DateTime.Today.Year;
        var gameNight = new Event { HostUserId = patrick.Id, HostName = patrick.Name, Name = "Fall Game Night", Address = "214 Maple Ave, Greenville, SC", MaxAttendees = 12, EventDateTime = new DateTime(year, 10, 31, 19, 0, 0), RsvpDeadline = new DateTime(year, 10, 28, 23, 59, 0) };
        var potluck = new Event { HostUserId = sarah.Id, HostName = sarah.Name, Name = "Friendsgiving Potluck", Address = "88 Pine St, Greenville, SC", MaxAttendees = 20, EventDateTime = new DateTime(year, 11, 21, 17, 30, 0), RsvpDeadline = new DateTime(year, 11, 14, 23, 59, 0) };
        var funRun = new Event { HostUserId = marcus.Id, HostName = marcus.Name, Name = "Charity 5K Fun Run", Address = "Cleveland Park, Greenville, SC", MaxAttendees = 50, EventDateTime = new DateTime(year, 11, 7, 8, 0, 0), RsvpDeadline = new DateTime(year, 11, 1, 23, 59, 0) };
        var studyGroup = new Event { HostUserId = patrick.Id, HostName = patrick.Name, Name = "Coding Study Group", Address = "ECPI University, Greenville, SC", MaxAttendees = 8, EventDateTime = new DateTime(year, 10, 24, 18, 0, 0), RsvpDeadline = new DateTime(year, 10, 23, 12, 0, 0) };
        var cookieSwap = new Event { HostUserId = linda.Id, HostName = linda.Name, Name = "Holiday Cookie Swap", Address = "502 Oak Ridge Dr, Simpsonville, SC", MaxAttendees = 6, EventDateTime = new DateTime(year, 12, 12, 14, 0, 0), RsvpDeadline = new DateTime(year, 12, 5, 23, 59, 0) };
        foreach (var ev in new[] { gameNight, potluck, funRun, studyGroup, cookieSwap })
            await AddEventAsync(ev);

        async Task Attend(Event ev, string name, string email, User? user = null) =>
            await _database!.InsertAsync(new Rsvp { EventId = ev.Id, UserId = user?.Id, Name = name, Email = email, Phone = user?.Phone ?? string.Empty });

        await Attend(gameNight, "Maria Lopez", "maria@example.com");
        await Attend(gameNight, "James Carter", "james@example.com");
        await Attend(gameNight, "Aisha Brown", "aisha@example.com");
        await Attend(gameNight, "Kevin Nguyen", "kevin@example.com");
        await Attend(potluck, patrick.Name, patrick.Email, patrick);
        await Attend(potluck, "Daniel Kim", "daniel@example.com");
        await Attend(potluck, "Emily Rivera", "emily@example.com");
        await Attend(funRun, patrick.Name, patrick.Email, patrick);
        await Attend(funRun, "Olivia Chen", "olivia@example.com");
        await Attend(funRun, "Noah Patel", "noah@example.com");
        await Attend(funRun, sarah.Name, sarah.Email, sarah);
        await Attend(studyGroup, "Tyler Brooks", "tyler@example.com");
        await Attend(studyGroup, "Hannah Scott", "hannah@example.com");
        foreach (var (n, e) in new[] { ("Rosa Diaz", "rosa@example.com"), ("Mark Evans", "mark@example.com"), ("Jenna Lee", "jenna@example.com"), ("Chris Adams", "chris@example.com"), ("Paula Gray", "paula@example.com"), ("Sam Fox", "sam@example.com") })
            await Attend(cookieSwap, n, e);
    }
}
