using FluentValidation;
using StackExchange.Redis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using TicketFlow.Api.Middleware;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// --- Logging ---
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console());

// --- JWT Bearer Authentication (Entra ID / OIDC standard) ---
// NOTE: Using JwtBearer directly — NOT Microsoft.Identity.Web.
// This keeps the code provider-agnostic per ADR-006.
// Swapping to Okta requires only changing the Authority and Audience values.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"{builder.Configuration["AzureAd:Instance"]}{builder.Configuration["AzureAd:TenantId"]}";
        options.Audience = builder.Configuration["AzureAd:Audience"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            // Entra ID v2 tokens use /v2.0 issuer
            ValidIssuers = new[]
            {
                $"https://login.microsoftonline.com/{builder.Configuration["AzureAd:TenantId"]}/v2.0",
                $"https://sts.windows.net/{builder.Configuration["AzureAd:TenantId"]}/"
            }
        };
    });

// --- Authorization policies ---
builder.Services.AddAuthorization(options =>
{
    // Default policy — any authenticated user
    options.AddPolicy("AuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());

    // Organizer — can create and manage events
    options.AddPolicy("OrganizerOrAdmin", policy =>
        policy.RequireRole("Organizer", "Admin"));

    // Admin only
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

// --- Application services (CQRS via MediatR) ---
var applicationAssembly = AppDomain.CurrentDomain.Load("TicketFlow.Application");
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
builder.Services.AddValidatorsFromAssembly(applicationAssembly);

// --- Infrastructure (EF Core + SQL Server) ---
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<IApplicationDbContext>(sp =>
    sp.GetRequiredService<ApplicationDbContext>());

// --- Background Job: release expired ticket reservations ---
builder.Services.AddHostedService<TicketFlow.Infrastructure.BackgroundJobs.ReservationExpiryJob>();

// --- API ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TicketFlow API", Version = "v1" });

    // Add JWT auth to Swagger UI
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Paste your JWT token here (without 'Bearer ' prefix)"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// --- CORS for Angular dev server ---
const string AngularCorsPolicy = "AngularDev";
builder.Services.AddCors(opts =>
{
    opts.AddPolicy(AngularCorsPolicy, policy => policy
     .WithOrigins(
         "http://localhost:4200",
         "https://app-ticketflow-web-dev-gv0o1o.azurewebsites.net")
     .AllowAnyHeader()
     .AllowAnyMethod());
});

// --- Redis ---
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(
        builder.Configuration["Redis__ConnectionString"] ?? "localhost:6379,abortConnect=false"));

builder.Services.AddSingleton<TicketFlow.Application.Common.Interfaces.IRedisService,
    TicketFlow.Infrastructure.Services.RedisService>();

var app = builder.Build();

// --- Pipeline ---
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Always run migrations (needed in Docker + future cloud deployments)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await TicketFlow.Infrastructure.Persistence.Seed.DatabaseSeeder.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseCors(AngularCorsPolicy);

// ORDER MATTERS: Authentication before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }



