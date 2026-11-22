using MdkHelpdesk.Application.Interfaces;
using MdkHelpdesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database & Infrastructure
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

// Retry transient SQL failures. Azure SQL serverless auto-pauses when idle and rejects the first
// connections (error 40613) for up to a minute while it resumes.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sql =>
        sql.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(15), errorNumbersToAdd: null)));

builder.Services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

var app = builder.Build();

// Apply pending EF Core migrations at startup when enabled (set in Azure App Service so the
// Azure SQL schema and seed data are created on deploy). Off by default for local development.
if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

// Placeholder endpoint until the API and Blazor UI are added.
app.MapGet("/", () => "MDK IT Helpdesk");

app.Run();
