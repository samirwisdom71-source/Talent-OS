using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.OpenApi;
using TalentSystem.Shared.Api;
using TalentSystem.Api.Middleware;
using TalentSystem.Application.DependencyInjection;
using TalentSystem.Application.Features.Identity.Interfaces;
using TalentSystem.Application.Features.Notifications.Interfaces;
using TalentSystem.Infrastructure.DependencyInjection;
using TalentSystem.Persistence.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
var trimmedOrigins = corsOrigins
    .Where(static o => !string.IsNullOrWhiteSpace(o))
    .Select(static o => o!.Trim())
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();
if (trimmedOrigins.Length > 0)
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(trimmedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });
}

builder.Services.AddControllers(options =>
{
    var defaultAuthPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(defaultAuthPolicy));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Talent OS API",
        Version = "v1",
        Description = "Enterprise Talent Management System foundation API."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document, null),
            new List<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsProduction())
{
    var signingKey = app.Configuration["Jwt:SigningKey"] ?? string.Empty;
    if (signingKey.Contains("LOCAL_DEV", StringComparison.OrdinalIgnoreCase) ||
        signingKey.Contains("REPLACE_IN_PRODUCTION", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException(
            "Jwt:SigningKey is still set to a development placeholder. Configure a strong secret before running in Production.");
    }
}

if (app.Configuration.GetValue("IdentitySeed:RunOnStartup", true))
{
    if (app.Environment.IsProduction() && app.Configuration.GetValue("IdentitySeed:BootstrapAdmin", false))
    {
        app.Logger.LogWarning(
            "IdentitySeed:BootstrapAdmin is enabled in Production. Create the admin user once, then set BootstrapAdmin to false and rotate the password.");
    }

    await using var scope = app.Services.CreateAsyncScope();
    var scoped = scope.ServiceProvider;
    await scoped.GetRequiredService<IIdentityDatabaseSeeder>().SeedAsync();
    await scoped.GetRequiredService<INotificationService>().EnsureDefaultTemplatesAsync();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Talent OS API v1");
    });
}
 

// In Development, skip HTTPS redirection so the HTTP Kestrel URL (e.g. :5042) stays on HTTP
// and browsers calling it from Next.js are not bounced to HTTPS (which often fails with self-signed certs).
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

if (trimmedOrigins.Length > 0)
{
    app.UseCors();
}
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Do not serve SPA index.html for unknown /api/* routes — that returns HTML with 200 and breaks JSON clients ("Http failure during parsing").
var webRoot = app.Environment.WebRootPath;
var indexHtmlPath = string.IsNullOrWhiteSpace(webRoot) ? null : Path.Combine(webRoot, "index.html");
app.MapFallback(async (HttpContext ctx) =>
{
    if (ctx.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
    {
        var traceId = Activity.Current?.Id ?? ctx.TraceIdentifier;
        ctx.Response.StatusCode = StatusCodes.Status404NotFound;
        var payload = ApiResponse<object>.FromFailure(
            new[] { "The requested API endpoint was not found." },
            traceId);
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        await ctx.Response.WriteAsJsonAsync(payload, jsonOptions);
        return;
    }

    if (indexHtmlPath is not null && File.Exists(indexHtmlPath))
    {
        ctx.Response.ContentType = "text/html; charset=utf-8";
        await ctx.Response.SendFileAsync(indexHtmlPath);
        return;
    }

    ctx.Response.StatusCode = StatusCodes.Status404NotFound;
});

app.Run();

public partial class Program;
