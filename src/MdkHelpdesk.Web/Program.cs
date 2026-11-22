var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Placeholder endpoint until the API and Blazor UI are added.
app.MapGet("/", () => "MDK IT Helpdesk");

app.Run();
