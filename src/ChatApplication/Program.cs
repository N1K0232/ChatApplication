using System.Text.Json.Serialization;
using ChatApplication.BusinessLayer.Settings;
using ChatApplication.Extensions;
using ChatApplication.Swagger;
using FluentValidation.AspNetCore;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.OpenApi.Models;
using MinimalHelpers.Validation;
using OperationResults.AspNetCore.Http;
using TinyHelpers.AspNetCore.Extensions;
using TinyHelpers.AspNetCore.Swagger;
using ResultErrorResponseFormat = OperationResults.AspNetCore.Http.ErrorResponseFormat;
using ValidationErrorResponseFormat = MinimalHelpers.Validation.ErrorResponseFormat;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", true, true);

var settings = builder.Services.ConfigureAndGet<AppSettings>(builder.Configuration, nameof(AppSettings));
var swagger = builder.Services.ConfigureAndGet<SwaggerSettings>(builder.Configuration, nameof(SwaggerSettings));

builder.Services.AddHttpContextAccessor();
builder.Services.AddRequestLocalization(settings.SupportedCultures);

builder.Services.AddRazorPages();
builder.Services.AddWebOptimizer(minifyCss: true, minifyJavaScript: builder.Environment.IsProduction());

builder.Services.AddDefaultExceptionHandler();
builder.Services.AddDefaultProblemDetails();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddOperationResult(options =>
{
    options.ErrorResponseFormat = ResultErrorResponseFormat.List;
});

builder.Services.ConfigureValidation(options =>
{
    options.ErrorResponseFormat = ValidationErrorResponseFormat.List;
});

builder.Services.AddFluentValidationAutoValidation(options =>
{
    options.DisableDataAnnotationsValidation = true;
});

if (swagger.Enabled)
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Chat Api",
            Version = "v1"
        });

        options.AddAcceptLanguageHeader();
        options.AddDefaultResponse();
    })
    .AddFluentValidationRulesToSwagger(options =>
    {
        options.SetNotNullableIfMinLengthGreaterThenZero = true;
    });
}

//builder.Services.Scan(scan => scan.FromAssemblyOf<UserService>()
//    .AddClasses(classes => classes.InNamespaceOf<UserService>())
//    .AsImplementedInterfaces()
//    .WithScopedLifetime());

var app = builder.Build();
app.Environment.ApplicationName = settings.ApplicationName;

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

app.UseStaticFiles();
app.UseDefaultFiles();

if (swagger.Enabled)
{
    app.UseMiddleware<SwaggerBasicAuthenticationMiddleware>();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Chat Api v1");
        options.InjectStylesheet("/css/swagger.css");
    });
}

app.UseWhen(context => context.IsApiRequest(), builder =>
{
    builder.UseExceptionHandler();
    builder.UseStatusCodePages();

    builder.UseAuthentication();
    builder.UseAuthorization();
});

app.MapRazorPages();
await app.RunAsync();