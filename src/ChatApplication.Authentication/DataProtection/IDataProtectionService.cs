namespace ChatApplication.Authentication.DataProtection;

public interface IDataProtectionService
{
    Task<string> ProtectAsync(string plaintext, TimeSpan lifetime);

    Task<string> UnprotectAsync(string protectedData);
}