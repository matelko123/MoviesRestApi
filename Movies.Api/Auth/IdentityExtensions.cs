using System.Security.Claims;

namespace Movies.Api.Auth;

public static class IdentityExtensions
{
    public static Guid? GetUserId(this HttpContext context)
    {
        Claim? userId = context.User.Claims.SingleOrDefault(x => x.Type == "userid");
        if (Guid.TryParse(userId?.Value, out Guid parsedId))
        {
            return parsedId;
        }

        return null;
    }
}