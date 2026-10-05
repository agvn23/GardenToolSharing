using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using GardenToolSharing.Api.Api.Endpoints;
using GardenToolSharing.Api.Api.Middleware;
using GardenToolSharing.Api.Application.Interfaces;
using GardenToolSharing.Api.Application.Services;
using GardenToolSharing.Api.Common;
using GardenToolSharing.Api.Infrastructure.Auth;
using GardenToolSharing.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton(TimeProvider.System);

// Enums travel as strings ("Public", "Available"); numbers are rejected
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));

// Errors -> RFC 7807 ProblemDetails
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// CORS (non-functional target): only the Vite dev server origin may call this API from a browser.
// Add the production frontend origin here too once it exists.
const string FrontendCorsPolicy = "Frontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Rate limiting (FR016): only endpoints that opt in via .RequireRateLimiting(...) are limited,
// so /health, /ready and every GET are exempt automatically just by not being tagged.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, ct) =>
    {
        TimeSpan? retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var ra) ? ra : null;

        if (retryAfter is { } delay)
            context.HttpContext.Response.Headers.RetryAfter = ((int)delay.TotalSeconds).ToString();

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Too many requests",
            Detail = retryAfter is { } d
                ? $"Rate limit exceeded. Try again in {d.TotalSeconds:F0} second(s)."
                : "Rate limit exceeded. Please slow down and try again shortly."
        };

        context.HttpContext.Response.ContentType = "application/problem+json";
        await context.HttpContext.Response.WriteAsJsonAsync(problem, ct);
    };

    // POST /loans specifically: 10 per user per minute
    options.AddPolicy("loan-write", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        httpContext.GetRateLimitPartitionKey(),
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));

    // Every other write endpoint (POST /tools, /memberships, /auth/*, /tools/{id}:return): 5 per minute.
    // Auth endpoints are anonymous, so they're partitioned by IP instead of user id.
    options.AddPolicy("default-write", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        httpContext.GetRateLimitPartitionKey(),
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});

// Override in Docker/Azure with the env var ConnectionStrings__Default
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// JWT settings: Jwt:Key comes from user-secrets locally, the Jwt__Key env var elsewhere
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key is missing or shorter than 32 characters. " +
        "Set it with 'dotnet user-secrets set \"Jwt:Key\" \"<random 32+ chars>\"' locally, " +
        "or the Jwt__Key environment variable in Docker/Azure.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IToolRepository, ToolRepository>();
builder.Services.AddScoped<IToolService, ToolService>();
builder.Services.AddScoped<IMembershipRepository, MembershipRepository>();
builder.Services.AddScoped<IMembershipService, MembershipService>();
builder.Services.AddScoped<ILoanRepository, LoanRepository>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<ILeaderboardRepository, LeaderboardRepository>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<DbSeeder>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages(); // ProblemDetails bodies for bare 401/404 responses

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // UI at /scalar/v1
}

// Applies pending migrations on startup in every environment (Docker/Azure included), so the
// container is self-contained: no separate `dotnet ef database update` step needed at deploy time.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    // Sample data is Development-only; Docker/Azure environments start with an empty database.
    if (app.Environment.IsDevelopment())
        await scope.ServiceProvider.GetRequiredService<DbSeeder>().SeedAsync();
}

app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy); // before auth/authorization, per ASP.NET Core's recommended order
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter(); // after auth, so the partition key can read the user id claim

var api = app.MapGroup("").RequireCors(FrontendCorsPolicy);

api.MapSystemEndpoints();
api.MapAuthEndpoints();
api.MapToolEndpoints();
api.MapMembershipEndpoints();
api.MapLoanEndpoints();
api.MapLeaderboardEndpoints();

app.Run();
