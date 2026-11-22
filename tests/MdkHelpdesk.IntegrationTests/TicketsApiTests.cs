using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MdkHelpdesk.Application.DTOs;
using MdkHelpdesk.Domain.Enums;

namespace MdkHelpdesk.IntegrationTests;

/// <summary>
/// End-to-end tests of the REST API: HTTP request -> controller -> service -> EF Core -> database.
/// The tests share one database, so each test works on tickets it creates itself and only reads
/// the seeded ones.
/// </summary>
public sealed class TicketsApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client;

    public TicketsApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTickets_ReturnsSeededTickets_WithStatusAsReadableString()
    {
        var response = await _client.GetAsync("/api/tickets");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"status\":\"InProgress\"", json);

        var tickets = JsonSerializer.Deserialize<List<TicketResponse>>(json, JsonOptions);
        Assert.NotNull(tickets);
        Assert.Contains(tickets, t => t.Id == 1 && t.AssignedUserName == "Alice Johnson");
    }

    [Fact]
    public async Task GetTicket_ReturnsTicket_WhenItExists()
    {
        var ticket = await _client.GetFromJsonAsync<TicketResponse>("/api/tickets/2", JsonOptions);

        Assert.NotNull(ticket);
        Assert.Equal("Request Outlook shared mailbox access", ticket.Title);
        Assert.Equal("Bob Martinez", ticket.AssignedUserName);
    }

    [Fact]
    public async Task GetTicket_Returns404_WhenItDoesNotExist()
    {
        var response = await _client.GetAsync("/api/tickets/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateTicket_Returns201WithLocation_AndTicketCanBeFetched()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);

        var response = await _client.PostAsJsonAsync("/api/tickets", new CreateTicketRequest
        {
            Title = "Integration test ticket",
            Description = "Verify full HTTP -> service -> database persistence.",
            AssignedUserId = 1
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<TicketResponse>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal(TicketStatus.Open, created.Status);
        Assert.True(created.CreatedAt >= before);
        Assert.Equal(created.CreatedAt, created.UpdatedAt);

        var fetched = await _client.GetFromJsonAsync<TicketResponse>(response.Headers.Location, JsonOptions);
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal("Alice Johnson", fetched.AssignedUserName);
    }

    [Fact]
    public async Task CreateTicket_Returns400WithFieldErrors_WhenTitleIsEmpty()
    {
        var response = await _client.PostAsJsonAsync("/api/tickets", new CreateTicketRequest
        {
            Title = "",
            Description = "Missing a title."
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Title is required.", body);
    }

    [Fact]
    public async Task CreateTicket_Returns400ProblemDetails_WhenAssigneeDoesNotExist()
    {
        var response = await _client.PostAsJsonAsync("/api/tickets", new CreateTicketRequest
        {
            Title = "Invalid assignee ticket",
            Description = "Should be rejected by the service and mapped to 400.",
            AssignedUserId = 9999
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Assigned user with ID 9999 was not found.", body);
        Assert.DoesNotContain("   at ", body); // no stack trace leaked
    }

    [Fact]
    public async Task UpdateTicket_ChangesStatus_AndAdvancesUpdatedAtOnly()
    {
        var created = await CreateTicketAsync("Ticket to update");

        var response = await _client.PutAsJsonAsync($"/api/tickets/{created.Id}", new UpdateTicketRequest
        {
            Title = "Ticket to update (in progress)",
            Description = "Now being worked on.",
            Status = TicketStatus.InProgress
        }, JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<TicketResponse>(JsonOptions);
        Assert.NotNull(updated);
        Assert.Equal("Ticket to update (in progress)", updated.Title);
        Assert.Equal(TicketStatus.InProgress, updated.Status);
        Assert.Equal(created.CreatedAt, updated.CreatedAt);
        Assert.True(updated.UpdatedAt > created.UpdatedAt);
    }

    [Fact]
    public async Task UpdateTicket_Returns400_WhenStatusIsNotAKnownValue()
    {
        var created = await CreateTicketAsync("Ticket with invalid status update");

        var response = await _client.PutAsync(
            $"/api/tickets/{created.Id}",
            JsonBody("""{"title":"Still valid","description":"Still valid.","status":"Done"}"""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTicket_Returns404_WhenTicketDoesNotExist()
    {
        var response = await _client.PutAsJsonAsync("/api/tickets/9999", new UpdateTicketRequest
        {
            Title = "Does not exist",
            Description = "Nothing to update.",
            Status = TicketStatus.Open
        }, JsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AssignTicket_AssignsThenUnassigns()
    {
        var created = await CreateTicketAsync("Ticket to assign");
        Assert.Null(created.AssignedUserId);

        var assignResponse = await _client.PatchAsync(
            $"/api/tickets/{created.Id}/assign", JsonBody("""{"assignedUserId":2}"""));
        Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);
        var assigned = await assignResponse.Content.ReadFromJsonAsync<TicketResponse>(JsonOptions);
        Assert.NotNull(assigned);
        Assert.Equal("Bob Martinez", assigned.AssignedUserName);

        var unassignResponse = await _client.PatchAsync(
            $"/api/tickets/{created.Id}/assign", JsonBody("""{"assignedUserId":null}"""));
        Assert.Equal(HttpStatusCode.OK, unassignResponse.StatusCode);
        var unassigned = await unassignResponse.Content.ReadFromJsonAsync<TicketResponse>(JsonOptions);
        Assert.NotNull(unassigned);
        Assert.Null(unassigned.AssignedUserId);
    }

    [Fact]
    public async Task AssignTicket_Returns400_WhenUserDoesNotExist()
    {
        var created = await CreateTicketAsync("Ticket with invalid assignment");

        var response = await _client.PatchAsync(
            $"/api/tickets/{created.Id}/assign", JsonBody("""{"assignedUserId":9999}"""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AssignTicket_Returns404_WhenTicketDoesNotExist()
    {
        var response = await _client.PatchAsync(
            "/api/tickets/9999/assign", JsonBody("""{"assignedUserId":1}"""));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTickets_FiltersByStatusAndAssignedUser()
    {
        var created = await CreateTicketAsync("Open ticket for Priya", assignedUserId: 3);

        var tickets = await _client.GetFromJsonAsync<List<TicketResponse>>(
            "/api/tickets?status=Open&assignedUserId=3", JsonOptions);

        Assert.NotNull(tickets);
        Assert.Contains(tickets, t => t.Id == created.Id);
        Assert.All(tickets, t =>
        {
            Assert.Equal(TicketStatus.Open, t.Status);
            Assert.Equal(3, t.AssignedUserId);
        });
    }

    [Fact]
    public async Task GetTickets_Returns400_WhenStatusFilterIsNotAKnownValue()
    {
        var response = await _client.GetAsync("/api/tickets?status=Done");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_ReturnsSeededUsersOrderedByName()
    {
        var users = await _client.GetFromJsonAsync<List<UserResponse>>("/api/users", JsonOptions);

        Assert.NotNull(users);
        Assert.Equal(
            new[] { "Alice Johnson", "Bob Martinez", "Daniel Kim", "Priya Patel" },
            users.Select(u => u.Name));
    }

    [Fact]
    public async Task GetTickets_UnassignedFilter_ReturnsOnlyTicketsWithNoAssignee()
    {
        var tickets = await _client.GetFromJsonAsync<List<TicketResponse>>(
            "/api/tickets?unassigned=true", JsonOptions);

        Assert.NotNull(tickets);
        Assert.Contains(tickets, t => t.Id == 4);
        Assert.All(tickets, t => Assert.Null(t.AssignedUserId));
    }

    [Fact]
    public async Task GetTickets_Returns400_WhenUserAndUnassignedFiltersAreCombined()
    {
        var response = await _client.GetAsync("/api/tickets?assignedUserId=1&unassigned=true");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<TicketResponse> CreateTicketAsync(string title, int? assignedUserId = null)
    {
        var response = await _client.PostAsJsonAsync("/api/tickets", new CreateTicketRequest
        {
            Title = title,
            Description = "Created by an integration test.",
            AssignedUserId = assignedUserId
        });
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<TicketResponse>(JsonOptions);
        return created!;
    }

    private static StringContent JsonBody(string json) => new(json, Encoding.UTF8, "application/json");
}
