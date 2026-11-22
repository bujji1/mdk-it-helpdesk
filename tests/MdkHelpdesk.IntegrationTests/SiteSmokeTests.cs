using System.Net;

namespace MdkHelpdesk.IntegrationTests;

/// <summary>
/// Smoke tests for the parts of the site outside the JSON API: the server-rendered UI and Swagger.
/// </summary>
public sealed class SiteSmokeTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SiteSmokeTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HomePage_RendersTheTicketList()
    {
        var response = await _client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Support Tickets", html);
    }

    [Fact]
    public async Task SwaggerDocument_DescribesTheTicketEndpoints()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("/api/Tickets/{id}/assign", json);
    }
}
