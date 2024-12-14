using ChatApplication.Authentication.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ChatApplication.Authorization.Requirements;

public class UserActiveHandler(UserManager<ApplicationUser> userManager) : AuthorizationHandler<UserActiveRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, UserActiveRequirement requirement)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            var user = await userManager.FindByNameAsync(context.User.Identity.Name);
            var lockedOut = await userManager.IsLockedOutAsync(user);

            if (!lockedOut)
            {
                context.Succeed(requirement);
            }
        }
    }
}