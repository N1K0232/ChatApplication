using ChatApplication.BusinessLayer.Services.Interfaces;
using ChatApplication.Shared.Models.Requests;
using ChatApplication.Shared.Models.Responses;
using MinimalHelpers.Routing;
using OperationResults.AspNetCore.Http;

namespace ChatApplication.Endpoints;

public class AuthEndpoint : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var authApiGroup = endpoints.MapGroup("/api/auth").AllowAnonymous();

        authApiGroup.MapPost("/login", LoginAsync)
            .Produces<AuthResponse>()
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("login")
            .WithOpenApi();

        authApiGroup.MapPost("/register", RegisterAsync)
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("register")
            .WithOpenApi();
    }

    private static async Task<IResult> LoginAsync(IIdentityService identityService, LoginRequest request, HttpContext httpContext)
    {
        var result = await identityService.LoginAsync(request);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> RegisterAsync(IIdentityService identityService, RegisterRequest request, HttpContext httpContext)
    {
        var result = await identityService.RegisterAsync(request);
        return httpContext.CreateResponse(result, StatusCodes.Status201Created);
    }
}