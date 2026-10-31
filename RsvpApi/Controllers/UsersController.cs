using Microsoft.AspNetCore.Mvc;
using RsvpApi.Data;
using RsvpApi.Filters;
using RsvpApi.Models;

namespace RsvpApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly RsvpRepository _repository;

    public UsersController(RsvpRepository repository) => _repository = repository;

    // POST api/Users - create an account
    [HttpPost]
    public ActionResult<UserDto> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Phone))
            return BadRequest("Name, e-mail address, password, and phone number are all required.");

        if (!request.Email.Contains('@') || !request.Email.Contains('.'))
            return BadRequest("Please enter a valid e-mail address.");

        if (request.Password.Length < 8)
            return BadRequest("Password must be at least 8 characters.");

        var user = _repository.AddUser(request.Name, request.Email, request.Password, request.Phone);
        if (user is null)
            return Conflict("An account with this e-mail address already exists.");

        var dto = new UserDto(user.Id, user.Name, user.Email, user.Phone);
        return CreatedAtAction(nameof(Me), dto);
    }

    // GET api/Users/me - log in: validates the Basic Authentication credentials and returns the account
    [HttpGet("me")]
    [BasicAuthentication]
    public ActionResult<UserDto> Me()
    {
        var user = HttpContext.AuthenticatedUser()!;
        return new UserDto(user.Id, user.Name, user.Email, user.Phone);
    }
}
