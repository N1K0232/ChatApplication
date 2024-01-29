using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ChatApplication.Authentication.Entities;
using ChatApplication.Authentication.Extensions;
using ChatApplication.BusinessLayer.Extensions;
using ChatApplication.BusinessLayer.Services.Interfaces;
using ChatApplication.BusinessLayer.Settings;
using ChatApplication.Shared.Models.Requests;
using ChatApplication.Shared.Models.Responses;
using FluentEmail.Core;
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

    private readonly IFluentEmail fluentEmail;
    private readonly JwtSettings jwtSettings;

    public IdentityService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
        IFluentEmail fluentEmail, IOptions<JwtSettings> jwtSettingsOptions)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.fluentEmail = fluentEmail;

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

        var response = CreateToken(claims);
        await SaveRefreshTokenAsync(user, response.RefreshToken);

        return response;
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var user = await ValidateAccessTokenAsync(request.AccessToken);
        if (user is not null)
        {
            var dbUser = await userManager.FindByIdAsync(user.GetClaimValue(ClaimTypes.NameIdentifier));
            if (dbUser?.RefreshToken is null || dbUser.RefreshTokenExpirationDate < DateTime.UtcNow || dbUser.RefreshToken != request.RefreshToken)
            {
                return Result.Fail(FailureReasons.ClientError, "Invalid refresh token");
            }

            var response = CreateToken(user.Claims);
            await SaveRefreshTokenAsync(dbUser, response.RefreshToken);

            return response;
        }

        return Result.Fail(FailureReasons.ClientError, "Invalid access token signature", "Couldn't verify the access token");
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
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        var response = await fluentEmail.To(user.Email)
            .Subject("Verify your email address")
            .Body($"Your email verification token:{Environment.NewLine}{token}").SendAsync();

        return result.Succeeded && response.Successful ?
            Result.Ok() :
            Result.Fail(FailureReasons.ClientError, "Registration failed", result.GetErrors() + response.GetErrors());
    }

    public async Task<Result> LogoutAsync()
    {
        var user = signInManager.Context.User;
        var dbUser = await userManager.FindByIdAsync(user.GetClaimValue(ClaimTypes.NameIdentifier));

        dbUser.RefreshToken = null;
        dbUser.RefreshTokenExpirationDate = null;

        await userManager.UpdateAsync(dbUser);
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
            DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        var refreshToken = GenerateRefreshToken();

        return new AuthResponse(accessToken, refreshToken);

        static string GenerateRefreshToken()
        {
            using var generator = RandomNumberGenerator.Create();
            var randomNumber = new byte[256];

            generator.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    private async Task SaveRefreshTokenAsync(ApplicationUser user, string refreshToken)
    {
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpirationDate = DateTime.UtcNow.AddMinutes(jwtSettings.RefreshTokenExpirationMinutes);

        await userManager.UpdateAsync(user);
    }

    private Task<ClaimsPrincipal> ValidateAccessTokenAsync(string accessToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecurityKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = false,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var user = handler.ValidateToken(accessToken, parameters, out var securityToken);
            if (securityToken is JwtSecurityToken jwtSecurityToken && jwtSecurityToken.Header.Alg == SecurityAlgorithms.HmacSha256)
            {
                return Task.FromResult(user);
            }
        }
        catch
        {
        }

        return Task.FromResult<ClaimsPrincipal>(null);
    }
}