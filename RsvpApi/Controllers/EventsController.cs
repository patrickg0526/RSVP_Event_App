using Microsoft.AspNetCore.Mvc;
using RsvpApi.Data;
using RsvpApi.Filters;
using RsvpApi.Models;

namespace RsvpApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EventsController : ControllerBase
{
    private readonly RsvpRepository _repository;

    public EventsController(RsvpRepository repository) => _repository = repository;

    // GET api/Events - all events (anyone, including guests)
    [HttpGet]
    public IEnumerable<EventDto> GetAll() => _repository.GetAllEvents();

    // GET api/Events/hosting - events the logged-in user is hosting
    [HttpGet("hosting")]
    [BasicAuthentication]
    public IEnumerable<EventDto> Hosting() => _repository.GetHostingEvents(HttpContext.AuthenticatedUser()!.Id);

    // GET api/Events/attending - events the logged-in user is attending
    [HttpGet("attending")]
    [BasicAuthentication]
    public IEnumerable<EventDto> Attending() => _repository.GetAttendingEvents(HttpContext.AuthenticatedUser()!);

    // GET api/Events/5
    [HttpGet("{id:int}")]
    public ActionResult<EventDto> Get(int id) =>
        _repository.GetEvent(id) is { } ev ? ev : NotFound();

    // GET api/Events/5/attendees - names of the people attending
    [HttpGet("{id:int}/attendees")]
    public ActionResult<IEnumerable<AttendeeDto>> Attendees(int id) =>
        _repository.GetEvent(id) is null ? NotFound() : _repository.GetAttendees(id);

    // GET api/Events/5/rsvp - whether the logged-in user has already RSVP'd
    [HttpGet("{id:int}/rsvp")]
    [BasicAuthentication]
    public ActionResult<RsvpStatusDto> RsvpStatus(int id) =>
        new RsvpStatusDto(_repository.HasRsvp(id, HttpContext.AuthenticatedUser()!.Email));

    // POST api/Events - add an event hosted by the logged-in user
    [HttpPost]
    [BasicAuthentication]
    public ActionResult<EventDto> Create([FromBody] CreateEventRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.HostName) || string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Address))
            return BadRequest("Please fill in all fields.");
        if (request.MaxAttendees < 1)
            return BadRequest("Maximum attendees must be a whole number greater than zero.");
        if (request.EventDateTime <= DateTime.Now)
            return BadRequest("The event date and time must be in the future.");
        if (request.RsvpDeadline <= DateTime.Now)
            return BadRequest("The RSVP deadline must be in the future.");
        if (request.RsvpDeadline > request.EventDateTime)
            return BadRequest("The RSVP deadline must be on or before the event date and time.");

        var created = _repository.AddEvent(new Event
        {
            HostUserId = HttpContext.AuthenticatedUser()!.Id,
            HostName = request.HostName.Trim(),
            Name = request.Name.Trim(),
            Address = request.Address.Trim(),
            MaxAttendees = request.MaxAttendees,
            EventDateTime = request.EventDateTime,
            RsvpDeadline = request.RsvpDeadline
        });
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    // POST api/Events/5/rsvps - RSVP for an event.
    // Logged-in users send their saved credentials; guests send name, e-mail, and phone.
    [HttpPost("{id:int}/rsvps")]
    [BasicAuthentication(Optional = true)]
    public IActionResult AddRsvp(int id, [FromBody] RsvpRequest request)
    {
        var user = HttpContext.AuthenticatedUser();
        string name = user?.Name ?? request.Name?.Trim() ?? string.Empty;
        string email = user?.Email ?? request.Email?.Trim() ?? string.Empty;
        string phone = (string.IsNullOrWhiteSpace(request.Phone) ? user?.Phone : request.Phone.Trim()) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(phone))
            return BadRequest("Please fill in all fields.");
        if (!email.Contains('@') || !email.Contains('.'))
            return BadRequest("Please enter a valid e-mail address.");

        var outcome = _repository.AddRsvp(new Rsvp { EventId = id, UserId = user?.Id, Name = name, Email = email, Phone = phone });
        return outcome switch
        {
            RsvpOutcome.Created => StatusCode(StatusCodes.Status201Created),
            RsvpOutcome.NotFound => NotFound("This event no longer exists."),
            RsvpOutcome.DeadlinePassed => BadRequest("The RSVP deadline for this event has passed."),
            RsvpOutcome.Full => Conflict("Sorry, this event is full."),
            _ => Conflict("An RSVP for this e-mail address already exists for this event.")
        };
    }
}
