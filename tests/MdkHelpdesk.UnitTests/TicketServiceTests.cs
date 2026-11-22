using MdkHelpdesk.Application.DTOs;
using MdkHelpdesk.Application.Services;
using MdkHelpdesk.Domain.Entities;
using MdkHelpdesk.Domain.Enums;
using MdkHelpdesk.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace MdkHelpdesk.UnitTests;

/// <summary>
/// Exercises TicketService against a real relational engine (SQLite in-memory), so queries,
/// constraints and the seeded data behave as they would in production.
/// </summary>
public sealed class TicketServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly TicketService _sut;

    public TicketServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();

        _sut = new TicketService(_db, NullLogger<TicketService>.Instance);
    }

    [Fact]
    public async Task GetTicketsAsync_ReturnsSeededTicketsWithAssigneeNames()
    {
        var tickets = await _sut.GetTicketsAsync();

        Assert.Equal(3, tickets.Count);

        var vpnTicket = Assert.Single(tickets, t => t.Id == 1);
        Assert.Equal("VPN connection drops intermittently", vpnTicket.Title);
        Assert.Equal(TicketStatus.Open, vpnTicket.Status);
        Assert.Equal("Alice Johnson", vpnTicket.AssignedUserName);
    }

    [Fact]
    public async Task GetTicketsAsync_ReturnsMostRecentlyUpdatedFirst()
    {
        _db.Tickets.Add(new Ticket
        {
            Title = "Laptop will not boot",
            Description = "Black screen after the latest Windows update."
        });
        await _db.SaveChangesAsync();

        var tickets = await _sut.GetTicketsAsync();

        Assert.Equal(4, tickets.Count);
        Assert.Equal("Laptop will not boot", tickets[0].Title);
    }

    [Fact]
    public async Task GetTicketsAsync_ReturnsNullAssigneeName_WhenTicketIsUnassigned()
    {
        _db.Tickets.Add(new Ticket
        {
            Title = "Unassigned triage ticket",
            Description = "Nobody has picked this up yet."
        });
        await _db.SaveChangesAsync();

        var tickets = await _sut.GetTicketsAsync();

        var unassigned = Assert.Single(tickets, t => t.Title == "Unassigned triage ticket");
        Assert.Null(unassigned.AssignedUserId);
        Assert.Null(unassigned.AssignedUserName);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsTicketWithAssigneeAndTimestamps()
    {
        var ticket = await _sut.GetByIdAsync(2);

        Assert.NotNull(ticket);
        Assert.Equal("Request Outlook shared mailbox access", ticket.Title);
        Assert.Equal(TicketStatus.InProgress, ticket.Status);
        Assert.Equal("Bob Martinez", ticket.AssignedUserName);
        Assert.NotEqual(default, ticket.CreatedAt);
        Assert.NotEqual(default, ticket.UpdatedAt);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenTicketDoesNotExist()
    {
        var ticket = await _sut.GetByIdAsync(9999);

        Assert.Null(ticket);
    }

    [Fact]
    public async Task CreateAsync_SetsOpenStatusAndUtcTimestamps()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);

        var created = await _sut.CreateAsync(new CreateTicketRequest
        {
            Title = "Printer jam on 2nd floor",
            Description = "Paper tray 2 is jammed.",
            AssignedUserId = 1
        });

        Assert.True(created.Id > 0);
        Assert.Equal(TicketStatus.Open, created.Status);
        Assert.Equal("Alice Johnson", created.AssignedUserName);
        Assert.True(created.CreatedAt >= before);
        Assert.Equal(created.CreatedAt, created.UpdatedAt);
    }

    [Fact]
    public async Task CreateAsync_AllowsUnassignedTicket()
    {
        var created = await _sut.CreateAsync(new CreateTicketRequest
        {
            Title = "New starter needs a laptop",
            Description = "Starts on Monday; no assignee yet."
        });

        Assert.Null(created.AssignedUserId);
        Assert.Null(created.AssignedUserName);
    }

    [Fact]
    public async Task CreateAsync_TrimsTitleAndDescription()
    {
        var created = await _sut.CreateAsync(new CreateTicketRequest
        {
            Title = "  Monitor flickers  ",
            Description = "  Happens after waking from sleep.  "
        });

        Assert.Equal("Monitor flickers", created.Title);
        Assert.Equal("Happens after waking from sleep.", created.Description);
    }

    [Fact]
    public async Task CreateAsync_ThrowsArgumentException_WhenAssignedUserDoesNotExist()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.CreateAsync(new CreateTicketRequest
            {
                Title = "Invalid assignee test",
                Description = "Should fail validation.",
                AssignedUserId = 9999
            }));

        Assert.Contains("9999", ex.Message);
        Assert.Equal(3, await _db.Tickets.CountAsync());
    }

    [Fact]
    public async Task UpdateAsync_ChangesDetailsAndStatus_AndAdvancesUpdatedAtOnly()
    {
        var original = await _sut.GetByIdAsync(1);
        Assert.NotNull(original);

        var updated = await _sut.UpdateAsync(1, new UpdateTicketRequest
        {
            Title = "  VPN drops every 15 minutes  ",
            Description = "Confirmed on two laptops.",
            Status = TicketStatus.InProgress
        });

        Assert.NotNull(updated);
        Assert.Equal("VPN drops every 15 minutes", updated.Title);
        Assert.Equal("Confirmed on two laptops.", updated.Description);
        Assert.Equal(TicketStatus.InProgress, updated.Status);
        Assert.Equal(original.CreatedAt, updated.CreatedAt);
        Assert.True(updated.UpdatedAt > original.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_KeepsExistingAssignee()
    {
        var updated = await _sut.UpdateAsync(2, new UpdateTicketRequest
        {
            Title = "Request Outlook shared mailbox access",
            Description = "Access granted; waiting for confirmation.",
            Status = TicketStatus.Closed
        });

        Assert.NotNull(updated);
        Assert.Equal(2, updated.AssignedUserId);
        Assert.Equal("Bob Martinez", updated.AssignedUserName);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenTicketDoesNotExist()
    {
        var updated = await _sut.UpdateAsync(9999, new UpdateTicketRequest
        {
            Title = "Does not exist",
            Description = "Nothing to update.",
            Status = TicketStatus.Open
        });

        Assert.Null(updated);
    }

    [Fact]
    public async Task AssignAsync_ReassignsTicketAndAdvancesUpdatedAt()
    {
        var original = await _sut.GetByIdAsync(1);
        Assert.NotNull(original);

        var updated = await _sut.AssignAsync(1, 3);

        Assert.NotNull(updated);
        Assert.Equal(3, updated.AssignedUserId);
        Assert.Equal("Priya Patel", updated.AssignedUserName);
        Assert.True(updated.UpdatedAt > original.UpdatedAt);
    }

    [Fact]
    public async Task AssignAsync_WithNull_UnassignsTicket()
    {
        var updated = await _sut.AssignAsync(1, null);

        Assert.NotNull(updated);
        Assert.Null(updated.AssignedUserId);
        Assert.Null(updated.AssignedUserName);
    }

    [Fact]
    public async Task AssignAsync_ThrowsArgumentException_AndKeepsAssignee_WhenUserDoesNotExist()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.AssignAsync(1, 9999));

        var unchanged = await _sut.GetByIdAsync(1);
        Assert.NotNull(unchanged);
        Assert.Equal(1, unchanged.AssignedUserId);
    }

    [Fact]
    public async Task AssignAsync_ReturnsNull_WhenTicketDoesNotExist()
    {
        var updated = await _sut.AssignAsync(9999, 1);

        Assert.Null(updated);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
