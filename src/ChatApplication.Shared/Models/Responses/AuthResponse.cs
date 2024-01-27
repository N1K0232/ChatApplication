namespace ChatApplication.Shared.Models.Responses;

public record class AuthResponse(string AccessToken, string RefreshToken);