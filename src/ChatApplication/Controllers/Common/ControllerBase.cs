using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

namespace ChatApplication.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public abstract class ControllerBase : Microsoft.AspNetCore.Mvc.ControllerBase
{
}