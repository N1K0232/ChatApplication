using System.Net.Http.Headers;
using System.Text;
using ChatApplication.BusinessLayer.Settings;
using ChatApplication.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using TinyHelpers.Extensions;

namespace ChatApplication.Swagger;

public class SwaggerBasicAuthenticationMiddleware(RequestDelegate next, IOptions<SwaggerSettings> swaggerSettingsOptions)
{
    private readonly SwaggerSettings swaggerSettings = swaggerSettingsOptions.Value;

    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (httpContext.IsSwaggerRequest() && swaggerSettings.UserName.HasValue() && swaggerSettings.Password.HasValue())
        {
            string authenticationHeader = httpContext.Request.Headers[HeaderNames.Authorization];
            if (authenticationHeader?.StartsWith("Basic ") ?? false)
            {
                var isAuthenticated = await AuthenticateAsync(authenticationHeader);
                if (isAuthenticated)
                {
                    await next.Invoke(httpContext);
                    return;
                }
            }

            httpContext.Response.Headers.WWWAuthenticate = new StringValues("Basic");
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
        else
        {
            await next.Invoke(httpContext);
        }
    }

    private Task<bool> AuthenticateAsync(string authenticationHeader)
    {
        if (TryGetCredentials(authenticationHeader, out var credentials))
        {
            var userName = credentials.ElementAtOrDefault(0);
            var password = credentials.ElementAtOrDefault(1);

            var isAuthenticated = userName.Equals(swaggerSettings.UserName) && password.Equals(swaggerSettings.Password);
            return Task.FromResult(isAuthenticated);
        }

        return Task.FromResult(false);
    }

    private static bool TryGetCredentials(string authenticationHeader, out string[] credentials)
    {
        try
        {
            var header = AuthenticationHeaderValue.Parse(authenticationHeader);
            var parameter = Convert.FromBase64String(header.Parameter);

            credentials = Encoding.UTF8.GetString(parameter).Split(':', count: 2);
            return true;
        }
        catch
        {
            credentials = null;
            return false;
        }
    }
}