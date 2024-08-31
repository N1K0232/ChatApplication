using System.Text.Json.Serialization;
using ChatApplication.BusinessLayer.Settings;
using ChatApplication.Extensions;
using ChatApplication.Swagger;
using Microsoft.OpenApi.Models;
using TinyHelpers.AspNetCore.Extensions;
using TinyHelpers.AspNetCore.Swagger;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", true, true);

var appSettings = builder.Services.ConfigureAndGet<AppSettings>(builder.Configuration, nameof(AppSettings));
var swaggerSettings = builder.Services.ConfigureAndGet<SwaggerSettings>(builder.Configuration, nameof(SwaggerSettings));

builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

builder.Services.AddRequestLocalization(appSettings.SupportedCultures);
builder.Services.AddRazorPages();

builder.Services.AddDefaultExceptionHandler();
builder.Services.AddDefaultProblemDetails();

builder.Services.AddWebOptimizer(minifyCss: true, minifyJavaScript: builder.Environment.IsProduction());
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

if (swaggerSettings.Enabled)
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "Chat Api", Version = "v1" });
        options.AddAcceptLanguageHeader();
        options.AddDefaultResponse();
    });
}

var app = builder.Build();
app.Environment.ApplicationName = appSettings.ApplicationName;

app.UseHttpsRedirection();
app.UseRequestLocalization();

app.UseRouting();
app.UseWebOptimizer();

app.UseWhen(context => context.IsWebRequest(), builder =>
{
    if (!app.Environment.IsDevelopment())
    {
        builder.UseExceptionHandler("/Errors/500");
        builder.UseHsts();
    }

    builder.UseStatusCodePagesWithRedirects("/Errors/{0}");
});

app.UseWhen(context => context.IsApiRequest(), builder =>
{
    builder.UseExceptionHandler();
    builder.UseStatusCodePages();
});

app.UseStaticFiles();
app.UseDefaultFiles();

if (swaggerSettings.Enabled)
{
    app.UseMiddleware<SwaggerBasicAuthenticationMiddleware>();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Chat Api v1");
        options.InjectStylesheet("/css/swagger.css");
    });
}

app.UseAuthorization();

app.MapRazorPages();

await app.RunAsync();