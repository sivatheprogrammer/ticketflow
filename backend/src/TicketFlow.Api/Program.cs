using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;
using TicketFlow.Api.Middleware;
using TicketFlow.Application.Common.Interfaces;
using TicketFlow.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// --- Logging ---
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console());

// --- Application services (CQRS via MediatR) ---
var applicationAssembly = AppDomain.CurrentDomain.Load("TicketFlow.Application");
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));
builder.Services.AddValidatorsFromAssembly(applicationAssembly);

// --- Infrastructure (EF Core + SQL Server) ---
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

// --- Background Job: release expired ticket reservations every minute ---
builder.Services.AddHostedService<TicketFlow.Infrastructure.BackgroundJobs.ReservationExpiryJob>();

// --- API ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TicketFlow API", Version = "v1" });
});

// --- CORS for Angular dev server ---
const string AngularCorsPolicy = "AngularDev";
builder.Services.AddCors(opts =>
{
    opts.AddPolicy(AngularCorsPolicy, policy => policy
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

// --- Pipeline ---
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Auto-migrate and seed in dev only
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await TicketFlow.Infrastructure.Persistence.Seed.DatabaseSeeder.SeedAsync(db);
}

app.UseSerilogRequestLogging();
app.UseCors(AngularCorsPolicy);
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program accessible to integration tests
public partial class Program { }
