using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RsvpApi.Data;
using RsvpApi.Models;

namespace RsvpApi.Filters;

// HTTP Basic Authentication against the Users table.
// [BasicAuthentication]                 -> credentials required; 401 if missing or invalid
// [BasicAuthentication(Optional=true)]  -> anonymous allowed; if credentials are sent they must be valid
// The authenticated user is stored in HttpContext.Items for the controller to use.
public class BasicAuthentication : ActionFilterAttribute
{
    public const string UserKey = "AuthenticatedUser";

    public bool Optional { get; set; }

    // Run before ASP.NET's automatic model-validation filter (Order -2000),
    // so unauthenticated requests get 401 rather than a 400 about the request body
    public BasicAuthentication() => Order = -3000;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        string header = context.HttpContext.Request.Headers.Authorization.ToString();

        if (string.IsNullOrEmpty(header))
        {
            if (!Optional)
                Challenge(context);
            return;
        }

        var parts = header.Split(' ');
        if (parts.Length != 2 || parts[0] != "Basic")
        {
            Challenge(context);
            return;
        }

        string credentials;
        try
        {
            credentials = Encoding.UTF8.GetString(Convert.FromBase64String(parts[1]));
        }
        catch (FormatException)
        {
            Challenge(context);
            return;
        }

        // Split on the first colon only, so passwords may contain ':'
        int separator = credentials.IndexOf(':');
        if (separator < 0)
        {
            Challenge(context);
            return;
        }

        var repository = context.HttpContext.RequestServices.GetRequiredService<RsvpRepository>();
        User? user = repository.ValidateCredentials(credentials[..separator], credentials[(separator + 1)..]);
        if (user is null)
        {
            Challenge(context);
            return;
        }

        context.HttpContext.Items[UserKey] = user;
        base.OnActionExecuting(context);
    }

    private static void Challenge(ActionExecutingContext context)
    {
        context.HttpContext.Response.Headers.WWWAuthenticate = "Basic realm=\"RSVP\"";
        context.Result = new UnauthorizedResult();
    }
}

public static class HttpContextUserExtensions
{
    public static User? AuthenticatedUser(this HttpContext context) =>
        context.Items.TryGetValue(BasicAuthentication.UserKey, out var user) ? user as User : null;
}
