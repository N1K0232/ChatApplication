namespace ChatApplication.Shared.Models.Requests;

public class RegisterRequest
{
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string UserName { get; set; }

    public string Password { get; set; } = null!;
}