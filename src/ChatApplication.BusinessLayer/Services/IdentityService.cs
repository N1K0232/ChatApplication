using System.Security.Claims;
using AutoMapper;
using ChatApplication.Authentication;
using ChatApplication.Authentication.Entities;
using ChatApplication.BusinessLayer.Services.Interfaces;
using ChatApplication.Shared.Models.Requests;
using ChatApplication.Shared.Models.Responses;
using Microsoft.AspNetCore.Identity;
using OperationResults;
using SimpleAuthentication.JwtBearer;

namespace ChatApplication.BusinessLayer.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly IJwtBearerService jwtBearerService;
    private readonly IMapper mapper;

    public IdentityService(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtBearerService jwtBearerService,
        IMapper mapper)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.jwtBearerService = jwtBearerService;
        this.mapper = mapper;
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        var result = await signInManager.PasswordSignInAsync(user, request.Password, false, false);

        if (!result.Succeeded)
        {
            await userManager.AccessFailedAsync(user);
            return Result.Fail(FailureReasons.ClientError, "Invalid email or password");
        }

        var userRoles = await userManager.GetRolesAsync(user);
        await userManager.UpdateSecurityStampAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.SerialNumber, user.SecurityStamp)
        }
        .Union(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        var accessToken = await jwtBearerService.CreateTokenAsync(user.UserName, claims.ToList());
        return new AuthResponse(accessToken);
    }

    public async Task<Result> RegisterAsync(RegisterRequest request)
    {
        var user = mapper.Map<ApplicationUser>(request);
        var result = await userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            result = await userManager.AddToRoleAsync(user, RoleNames.User);
        }

        return result.Succeeded ? Result.Ok() : Result.Fail(FailureReasons.ClientError, string.Join(',', result.Errors.Select(e => e.Description)));
    }
}