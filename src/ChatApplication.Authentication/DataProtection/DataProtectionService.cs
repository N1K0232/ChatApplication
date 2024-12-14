using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChatApplication.Authentication.DataProtection;

public class DataProtectionService : IDataProtectionService
{
    private readonly IDataProtectionKeyContext context;
    private readonly ITimeLimitedDataProtector protector;

    public DataProtectionService(IDataProtectionKeyContext context, ITimeLimitedDataProtector protector)
    {
        this.context = context;
        this.protector = protector;
    }

    public async Task<string> ProtectAsync(string plaintext, TimeSpan lifetime)
    {
        await ThrowIfKeyNotExistsAsync();
        return protector.Protect(plaintext, lifetime);
    }

    public async Task<string> UnprotectAsync(string protectedData)
    {
        await ThrowIfKeyNotExistsAsync();
        return protector.Unprotect(protectedData);
    }

    private async Task ThrowIfKeyNotExistsAsync()
    {
        var keyExists = await context.DataProtectionKeys.AnyAsync();
        if (!keyExists)
        {
            throw new ApplicationException("There's no key stored that can be used to encrypt your message");
        }
    }
}