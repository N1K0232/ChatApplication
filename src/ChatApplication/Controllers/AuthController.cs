using ChatApplication.BusinessLayer.Services.Interfaces;
using ChatApplication.Shared.Models.Requests;
using ChatApplication.Shared.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OperationResults.AspNetCore;

namespace ChatApplication.Controllers;

public class AuthController : ControllerBase
{
    private readonly IIdentityService identityService;

    public AuthController(IIdentityService identityService)
    {
        this.identityService = identityService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await identityService.LoginAsync(request);
        return HttpContext.CreateResponse(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await identityService.RefreshTokenAsync(request);
        return HttpContext.CreateResponse(result);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await identityService.RegisterAsync(request);
        return HttpContext.CreateResponse(result, StatusCodes.Status200OK);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout()
    {
        var result = await identityService.LogoutAsync();
        return HttpContext.CreateResponse(result);
    }
}