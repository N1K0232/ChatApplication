using System.Net.Http.Headers;
using System.Text;
using ChatApplication.BusinessLayer.Settings;
using ChatApplication.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using TinyHelpers.Extensions;

namespace ChatApplication.Swagger;

public class SwaggerAuthenticationMiddleware
{
    private readonly RequestDelegate next;
    private readonly SwaggerSettings swaggerSettings;

    public SwaggerAuthenticationMiddleware(RequestDelegate next, IOptions<SwaggerSettings> swaggerSettingsOptions)
    {
        this.next = next;
        swaggerSettings = swaggerSettingsOptions.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.IsSwaggerRequest() && swaggerSettings.UserName.HasValue() && swaggerSettings.Password.HasValue())
        {
            string authenticationHeader = context.Request.Headers[HeaderNames.Authorization];
            if (authenticationHeader?.StartsWith("Basic ") ?? false)
            {
                // Get the credentials from request header
                var header = AuthenticationHeaderValue.Parse(authenticationHeader);
                var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(header.Parameter)).Split(':', count: 2);
                var userName = credentials.ElementAtOrDefault(0);
                var password = credentials.ElementAtOrDefault(1);

                // validate credentials
                if (userName == swaggerSettings.UserName && password == swaggerSettings.Password)
                {
                    await next.Invoke(context).ConfigureAwait(false);
                    return;
                }
            }

            context.Response.Headers.WWWAuthenticate = new StringValues("Basic");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
        else
        {
            await next.Invoke(context);
        }
    }
}