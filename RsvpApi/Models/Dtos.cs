namespace RsvpApi.Models;

// Shapes of the JSON sent to and returned from the API

public record UserDto(int Id, string Name, string Email, string Phone);

public record RegisterRequest(string Name, string Email, string Password, string Phone);

public record EventDto(
    int Id,
    int HostUserId,
    string HostName,
    string Name,
    string Address,
    int MaxAttendees,
    DateTime EventDateTime,
    DateTime RsvpDeadline,
    int CurrentAttendees);

public record CreateEventRequest(
    string HostName,
    string Name,
    string Address,
    int MaxAttendees,
    DateTime EventDateTime,
    DateTime RsvpDeadline);

// Attendee names only; e-mail addresses and phone numbers are not shared with other users
public record AttendeeDto(string Name, DateTime RsvpDate);

public record RsvpRequest(string? Name, string? Email, string? Phone);

public record RsvpStatusDto(bool Attending);
