using System.Security.Claims;
using System.Security.Principal;

namespace ChatApplication.Authentication.Extensions;

public static class ClaimsExtensions
{
    public static Guid GetId(this IPrincipal user)
    {
        var value = GetClaimValue(user, ClaimTypes.NameIdentifier);
        if (Guid.TryParse(value, out var id))
        {
            return id;
        }

        return Guid.Empty;
    }

    public static string GetFirstName(this IPrincipal user)
        => GetClaimValue(user, ClaimTypes.GivenName);

    public static string GetLastName(this IPrincipal user)
        => GetClaimValue(user, ClaimTypes.Surname);

    public static string GetEmail(this IPrincipal user)
        => GetClaimValue(user, ClaimTypes.Email);

    public static string GetUserName(this IPrincipal user)
        => GetClaimValue(user, ClaimTypes.Name);

    public static string GetClaimValue(this IPrincipal user, string claimType)
    {
        var value = ((ClaimsPrincipal)user).FindFirstValue(claimType);
        return value;
    }
}