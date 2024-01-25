using ChatApplication.Shared.Models.Requests;
using ChatApplication.Shared.Models.Responses;
using OperationResults;

namespace ChatApplication.BusinessLayer.Services.Interfaces;

public interface IIdentityService
{
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);

    Task<Result> RegisterAsync(RegisterRequest request);

    Task<Result> LogoutAsync();
}