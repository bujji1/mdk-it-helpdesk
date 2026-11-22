using MdkHelpdesk.Application.Interfaces;
using MdkHelpdesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database & Infrastructure
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

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
