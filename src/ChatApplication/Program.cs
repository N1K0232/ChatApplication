using System.Diagnostics;
using System.Reflection;
using System.Text;
using ChatApplication;
using ChatApplication.Authentication;
using ChatApplication.Authentication.Entities;
using ChatApplication.Authentication.Handlers;
using ChatApplication.Authentication.Requirements;
using ChatApplication.BusinessLayer.Contracts;
using ChatApplication.BusinessLayer.MapperProfiles;
using ChatApplication.BusinessLayer.Services;
using ChatApplication.BusinessLayer.Services.Interfaces;
using ChatApplication.BusinessLayer.Settings;
using ChatApplication.BusinessLayer.StartupServices;
using ChatApplication.BusinessLayer.Validations;
using ChatApplication.DataAccessLayer;
using ChatApplication.ExceptionHandlers;
using ChatApplication.Extensions;
using ChatApplication.StorageProviders.Extensions;
using ChatApplication.Swagger;
using FluentValidation;
using FluentValidation.AspNetCore;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using OperationResults.AspNetCore;
using Serilog;
using TinyHelpers.AspNetCore.Extensions;
using TinyHelpers.AspNetCore.Swagger;

var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder.Services, builder.Configuration, builder.Environment, builder.Host);

var app = builder.Build();
Configure(app, app.Environment, app.Services);

await app.RunAsync();

void ConfigureServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment, IHostBuilder host)
{
    var appSettings = services.ConfigureAndGet<AppSettings>(configuration, nameof(AppSettings));
    var jwtSettings = services.ConfigureAndGet<JwtSettings>(configuration, nameof(JwtSettings));
    var swaggerSettings = services.ConfigureAndGet<SwaggerSettings>(configuration, nameof(SwaggerSettings));

    host.UseSerilog((hostingContext, loggerConfiguration) =>
    {
        loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
    });

    services.AddHttpContextAccessor();
    services.AddMemoryCache();

    services.AddExceptionHandler<DefaultExceptionHandler>();
    services.AddProblemDetails(options =>
    {
        options.CustomizeProblemDetails = context =>
        {
            var statusCode = context.ProblemDetails.Status.GetValueOrDefault(StatusCodes.Status500InternalServerError);
            context.ProblemDetails.Type ??= $"https://httpstatuses.io/{statusCode}";
            context.ProblemDetails.Title ??= ReasonPhrases.GetReasonPhrase(statusCode);
            context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;
            context.ProblemDetails.Extensions["traceId"] = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
        };
    });

    services.AddWebOptimizer(minifyCss: true, minifyJavaScript: environment.IsProduction());
    services.AddRequestLocalization(appSettings.SupportedCultures);

    services.AddAutoMapper(typeof(UserMapperProfile).Assembly);
    services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

    services.AddFluentValidationAutoValidation(options =>
    {
        options.DisableDataAnnotationsValidation = true;
    });

    services.AddOperationResult(options =>
    {
        options.ErrorResponseFormat = ErrorResponseFormat.List;
    });

    if (swaggerSettings.Enabled)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "ChatApplication API", Version = "v1" });
            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Insert JWT token with the \"Bearer \" prefix",
                Name = HeaderNames.Authorization,
                Type = SecuritySchemeType.ApiKey
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        }
                    },
                    Array.Empty<string>()
                }
            });

            options.AddAcceptLanguageHeader();
            options.AddDefaultResponse();

            options.OperationFilter<AuthResponseOperationFilter>();

            options.MapType<DateTime>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "date-time",
                Example = new OpenApiString(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"))
            });

            options.MapType<DateOnly>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "date",
                Example = new OpenApiString(DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"))
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            options.UseAllOfToExtendReferenceSchemas();
            options.IncludeXmlComments(xmlPath);
        })
        .AddFluentValidationRulesToSwagger(options =>
        {
            options.SetNotNullableIfMinLengthGreaterThenZero = true;
        });
    }

    services.AddControllers();
    services.AddRazorPages();

    services.AddSqlServer<ApplicationDbContext>(configuration.GetConnectionString("SqlConnection"));
    services.AddSqlServer<AuthenticationDbContext>(configuration.GetConnectionString("SqlConnection"));

    services.AddScoped<IApplicationDbContext>(services => services.GetRequiredService<ApplicationDbContext>());
    services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
    })
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<AuthenticationDbContext>();

    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddCookie("JwtBearerHandler")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecurityKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

    services.AddAuthorization(options =>
    {
        var policyBuilder = new AuthorizationPolicyBuilder().RequireAuthenticatedUser();
        policyBuilder.Requirements.Add(new UserActiveRequirement());

        options.DefaultPolicy = policyBuilder.Build();
    });

    services.AddScoped<IAuthorizationHandler, UserActiveHandler>();
    services.AddScoped<IUserService, HttpUserService>();

    if (environment.IsDevelopment())
    {
        services.AddFileSystemStorage(options =>
        {
            options.SiteRootFolder = environment.ContentRootPath;
            options.StorageFolder = appSettings.StorageFolder;
        });
    }
    else
    {
        services.AddAzureStorage(options =>
        {
            options.ConnectionString = configuration.GetConnectionString("AzureStorageConnection");
            options.ContainerName = appSettings.ContainerName;
        });
    }

    services.AddScoped<IIdentityService, IdentityService>();
    services.AddScoped<IAuthenticatedService, AuthenticatedService>();

    services.AddHostedService<IdentityStartupService>();
}

void Configure(IApplicationBuilder app, IWebHostEnvironment environment, IServiceProvider services)
{
    var appSettings = services.GetRequiredService<IOptions<AppSettings>>().Value;
    var swaggerSettings = services.GetRequiredService<IOptions<SwaggerSettings>>().Value;

    environment.ApplicationName = appSettings.ApplicationName;

    app.UseHttpsRedirection();
    app.UseRequestLocalization();

    app.UseRouting();
    app.UseWebOptimizer();

    app.UseWhen(context => context.IsWebRequest(), builder =>
    {
        if (!environment.IsDevelopment())
        {
            builder.UseExceptionHandler("/Errors/500");
            builder.UseHsts();
        }

        builder.UseStatusCodePagesWithReExecute("/Errors/{0}");
    });

    app.UseWhen(context => context.IsApiRequest(), builder =>
    {
        builder.UseExceptionHandler();
        builder.UseStatusCodePages();

        builder.UseAuthentication();
        builder.UseAuthorization();
    });

    app.UseDefaultFiles();
    app.UseStaticFiles();

    if (swaggerSettings.Enabled)
    {
        app.UseMiddleware<SwaggerAuthenticationMiddleware>();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "ChatApplication API v1");
            options.InjectStylesheet("/css/swagger.css");
        });
    }

    app.UseSerilogRequestLogging(options =>
    {
        options.IncludeQueryInRequestPath = true;
    });

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        endpoints.MapRazorPages();
    });
}