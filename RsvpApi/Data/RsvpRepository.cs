using SQLite;
using RsvpApi.Models;
using RsvpApi.Services;

namespace RsvpApi.Data;

public enum RsvpOutcome { Created, NotFound, DeadlinePassed, Full, Duplicate }

// Server-side data access layer. This is the Week 3 app DAL, moved behind the web service.
// Registered as a singleton; SQLite access is serialized with a lock.
public class RsvpRepository
{
    private readonly SQLiteConnection _db;
    private readonly object _lock = new();

    public RsvpRepository(IConfiguration config)
    {
        string fileName = config["Database:FileName"] ?? "RsvpApi.db3";
        string path = Path.Combine(AppContext.BaseDirectory, fileName);

        _db = new SQLiteConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);
        _db.CreateTable<User>();
        _db.CreateTable<Event>();
        _db.CreateTable<Rsvp>();

        if (_db.Table<User>().Count() == 0)
            Seed();
    }

    // ---------- Users ----------

    public User? GetUserByEmail(string email)
    {
        string normalized = email.Trim();
        lock (_lock)
            return _db.Table<User>().Where(u => u.Email == normalized).FirstOrDefault();
    }

    public User? AddUser(string name, string email, string password, string phone)
    {
        lock (_lock)
        {
            if (GetUserByEmail(email) is not null)
                return null;

            var (hash, salt) = PasswordHasher.HashPassword(password);
            var user = new User
            {
                Name = name.Trim(),
                Email = email.Trim(),
                PasswordHash = hash,
                PasswordSalt = salt,
                Phone = phone.Trim()
            };
            _db.Insert(user);
            return user;
        }
    }

    public User? ValidateCredentials(string email, string password)
    {
        var user = GetUserByEmail(email);
        if (user is null)
            return null;
        return PasswordHasher.Verify(password, user.PasswordHash, user.PasswordSalt) ? user : null;
    }

    // ---------- Events ----------

    public List<EventDto> GetAllEvents()
    {
        lock (_lock)
            return ToDtos(_db.Table<Event>().OrderBy(e => e.EventDateTime).ToList());
    }

    public List<EventDto> GetHostingEvents(int userId)
    {
        lock (_lock)
            return ToDtos(_db.Table<Event>().Where(e => e.HostUserId == userId).OrderBy(e => e.EventDateTime).ToList());
    }

    public List<EventDto> GetAttendingEvents(User user)
    {
        lock (_lock)
        {
            var events = _db.Query<Event>(
                "SELECT e.* FROM Events e JOIN Rsvps r ON r.EventId = e.Id " +
                "WHERE r.UserId = ? OR r.Email = ? COLLATE NOCASE ORDER BY e.EventDateTime",
                user.Id, user.Email);
            return ToDtos(events);
        }
    }

    public EventDto? GetEvent(int id)
    {
        lock (_lock)
        {
            var ev = _db.Find<Event>(id);
            return ev is null ? null : ToDtos(new List<Event> { ev })[0];
        }
    }

    public EventDto AddEvent(Event ev)
    {
        lock (_lock)
        {
            ev.Id = 0;
            ev.CreatedAt = DateTime.Now;
            _db.Insert(ev);
            return ToDtos(new List<Event> { ev })[0];
        }
    }

    // ---------- RSVPs ----------

    public List<AttendeeDto> GetAttendees(int eventId)
    {
        lock (_lock)
            return _db.Table<Rsvp>()
                .Where(r => r.EventId == eventId)
                .OrderBy(r => r.RsvpDate)
                .ToList()
                .Select(r => new AttendeeDto(r.Name, r.RsvpDate))
                .ToList();
    }

    public bool HasRsvp(int eventId, string email)
    {
        string normalized = email.Trim();
        lock (_lock)
            return _db.Table<Rsvp>().Where(r => r.EventId == eventId && r.Email == normalized).Count() > 0;
    }

    // Enforces the project rules: before the deadline, not full, and no duplicate RSVPs.
    // The checks and the insert happen under one lock so two requests can't both take the last spot.
    public RsvpOutcome AddRsvp(Rsvp rsvp)
    {
        lock (_lock)
        {
            var ev = _db.Find<Event>(rsvp.EventId);
            if (ev is null)
                return RsvpOutcome.NotFound;
            if (DateTime.Now > ev.RsvpDeadline)
                return RsvpOutcome.DeadlinePassed;
            int current = _db.Table<Rsvp>().Where(r => r.EventId == ev.Id).Count();
            if (current >= ev.MaxAttendees)
                return RsvpOutcome.Full;
            if (HasRsvp(rsvp.EventId, rsvp.Email))
                return RsvpOutcome.Duplicate;

            rsvp.Id = 0;
            rsvp.RsvpDate = DateTime.Now;
            _db.Insert(rsvp);
            return RsvpOutcome.Created;
        }
    }

    // ---------- Helpers ----------

    private List<EventDto> ToDtos(List<Event> events)
    {
        var counts = _db.Query<EventCount>("SELECT EventId, COUNT(*) AS Total FROM Rsvps GROUP BY EventId")
            .ToDictionary(c => c.EventId, c => c.Total);

        return events.Select(e => new EventDto(e.Id, e.HostUserId, e.HostName, e.Name, e.Address, e.MaxAttendees,
            e.EventDateTime, e.RsvpDeadline, counts.GetValueOrDefault(e.Id))).ToList();
    }

    private class EventCount
    {
        public int EventId { get; set; }
        public int Total { get; set; }
    }

    private void Seed()
    {
        var patrick = AddUser("Patrick Gonzalez", "patrick@rsvpapp.com", "Password1", "555-123-4567")!;
        var sarah = AddUser("Sarah Mitchell", "sarah@example.com", "Password1", "555-201-3344")!;
        var marcus = AddUser("Marcus Johnson", "marcus@example.com", "Password1", "555-301-7788")!;
        var linda = AddUser("Linda Torres", "linda@example.com", "Password1", "555-401-9900")!;

        // Dates are relative to today so the sample events stay in the future
        DateTime today = DateTime.Today;
        Event Make(User host, string name, string address, int max, int daysOut, int hour, int deadlineDaysBefore) => new()
        {
            HostUserId = host.Id, HostName = host.Name, Name = name, Address = address, MaxAttendees = max,
            EventDateTime = today.AddDays(daysOut).AddHours(hour),
            RsvpDeadline = today.AddDays(daysOut - deadlineDaysBefore).AddHours(23).AddMinutes(59)
        };

        var gameNight = Make(patrick, "Fall Game Night", "214 Maple Ave, Greenville, SC", 12, 10, 19, 3);
        var potluck = Make(sarah, "Friendsgiving Potluck", "88 Pine St, Greenville, SC", 20, 21, 17, 7);
        var funRun = Make(marcus, "Charity 5K Fun Run", "Cleveland Park, Greenville, SC", 50, 7, 8, 6);
        var studyGroup = Make(patrick, "Coding Study Group", "ECPI University, Greenville, SC", 8, 4, 18, 1);
        var cookieSwap = Make(linda, "Holiday Cookie Swap", "502 Oak Ridge Dr, Simpsonville, SC", 6, 42, 14, 7);
        foreach (var ev in new[] { gameNight, potluck, funRun, studyGroup, cookieSwap })
            _db.Insert(ev);

        void Attend(Event ev, string name, string email, User? user = null) =>
            _db.Insert(new Rsvp { EventId = ev.Id, UserId = user?.Id, Name = name, Email = email, Phone = user?.Phone ?? string.Empty });

        Attend(gameNight, "Maria Lopez", "maria@example.com");
        Attend(gameNight, "James Carter", "james@example.com");
        Attend(gameNight, "Aisha Brown", "aisha@example.com");
        Attend(gameNight, "Kevin Nguyen", "kevin@example.com");
        Attend(potluck, patrick.Name, patrick.Email, patrick);
        Attend(potluck, "Daniel Kim", "daniel@example.com");
        Attend(potluck, "Emily Rivera", "emily@example.com");
        Attend(funRun, patrick.Name, patrick.Email, patrick);
        Attend(funRun, "Olivia Chen", "olivia@example.com");
        Attend(funRun, "Noah Patel", "noah@example.com");
        Attend(funRun, sarah.Name, sarah.Email, sarah);
        Attend(studyGroup, "Tyler Brooks", "tyler@example.com");
        Attend(studyGroup, "Hannah Scott", "hannah@example.com");
        foreach (var (n, e) in new[] { ("Rosa Diaz", "rosa@example.com"), ("Mark Evans", "mark@example.com"), ("Jenna Lee", "jenna@example.com"), ("Chris Adams", "chris@example.com"), ("Paula Gray", "paula@example.com"), ("Sam Fox", "sam@example.com") })
            Attend(cookieSwap, n, e);
    }
}
