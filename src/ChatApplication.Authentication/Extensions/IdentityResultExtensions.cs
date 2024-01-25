using Microsoft.AspNetCore.Identity;

namespace ChatApplication.Authentication.Extensions;

public static class IdentityResultExtensions
{
    public static string GetErrors(this IdentityResult result)
        => string.Join(",", result.Errors.Select(e => e.Description));
}