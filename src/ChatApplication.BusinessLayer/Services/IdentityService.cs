using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChatApplication.Authentication;
using ChatApplication.Authentication.Entities;
using ChatApplication.Authentication.Extensions;
using ChatApplication.BusinessLayer.Services.Interfaces;
using ChatApplication.BusinessLayer.Settings;
using ChatApplication.Shared.Models.Requests;
using ChatApplication.Shared.Models.Responses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OperationResults;

namespace ChatApplication.BusinessLayer.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly JwtSettings jwtSettings;

    public IdentityService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
        IOptions<JwtSettings> jwtSettingsOptions)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        jwtSettings = jwtSettingsOptions.Value;
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var result = await signInManager.PasswordSignInAsync(request.UserName, request.Password, false, false);
        if (!result.Succeeded)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid username or password");
        }

        var user = await userManager.FindByNameAsync(request.UserName);
        await userManager.UpdateSecurityStampAsync(user);

        var userRoles = await userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.SerialNumber, user.SecurityStamp ?? string.Empty)
        }.Union(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        return CreateToken(claims);
    }

    public async Task<Result> RegisterAsync(RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.UserName
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (result.Succeeded)
        {
            result = await userManager.AddToRoleAsync(user, RoleNames.User);
        }

        return result.Succeeded ? Result.Ok() : Result.Fail(FailureReasons.ClientError, "Registration failed", result.GetErrors());
    }

    public async Task<Result> LogoutAsync()
    {
        await signInManager.SignOutAsync();
        await signInManager.Context.SignOutAsync("JwtBearerHandler");

        return Result.Ok();
    }

    private AuthResponse CreateToken(IEnumerable<Claim> claims)
    {
        var securityKey = Encoding.UTF8.GetBytes(jwtSettings.SecurityKey);
        var symmetricSecurityKey = new SymmetricSecurityKey(securityKey);

        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
        var jwtSecurityToken = new JwtSecurityToken
        (
            jwtSettings.Issuer,
            jwtSettings.Audience,
            claims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationMinutes),
            signingCredentials
        );

        var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        var accessToken = jwtSecurityTokenHandler.WriteToken(jwtSecurityToken);

        return new AuthResponse(accessToken);
    }
}