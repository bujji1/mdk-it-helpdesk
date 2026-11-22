using MdkHelpdesk.Application.Services;
using MdkHelpdesk.Domain.Entities;
using MdkHelpdesk.Domain.Enums;
using MdkHelpdesk.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

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

        _sut = new TicketService(_db);
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

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
