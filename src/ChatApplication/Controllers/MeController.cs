using ChatApplication.BusinessLayer.Services.Interfaces;
using ChatApplication.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OperationResults.AspNetCore;

namespace ChatApplication.Controllers;

public class MeController : ControllerBase
{
    private readonly IAuthenticatedService authenticatedService;

    public MeController(IAuthenticatedService authenticatedService)
    {
        this.authenticatedService = authenticatedService;
    }

    [HttpGet("GetMe")]
    [Authorize]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        var result = await authenticatedService.GetAsync();
        return HttpContext.CreateResponse(result);
    }
}