using ChatApplication.BusinessLayer.Services.Interfaces;
using ChatApplication.Shared.Models;
using MinimalHelpers.Routing;
using OperationResults.AspNetCore.Http;

namespace ChatApplication.Endpoints;

public class MeEndpoint : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var meApiGroup = endpoints.MapGroup("/api/me").RequireAuthorization();

        meApiGroup.MapGet(string.Empty, GetMeAsync)
            .Produces<User>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithName("profile")
            .WithOpenApi();
    }

    private static async Task<IResult> GetMeAsync(IUserService userService, HttpContext httpContext)
    {
        var result = await userService.GetAsync();
        return httpContext.CreateResponse(result);
    }
}