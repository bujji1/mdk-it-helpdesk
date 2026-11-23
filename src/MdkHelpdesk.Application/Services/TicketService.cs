using MdkHelpdesk.Application.DTOs;
using MdkHelpdesk.Application.Interfaces;
using MdkHelpdesk.Domain.Entities;
using MdkHelpdesk.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MdkHelpdesk.Application.Services;

/// <summary>
/// Implements core helpdesk ticket workflows with validation, structured logging, and EF Core persistence.
/// </summary>
public sealed class TicketService : ITicketService
{
    private readonly IAppDbContext _db;
    private readonly ILogger<TicketService> _logger;

    public TicketService(IAppDbContext db, ILogger<TicketService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TicketResponse>> GetTicketsAsync(
        TicketStatus? status = null,
        int? assignedUserId = null,
        bool unassignedOnly = false,
        CancellationToken cancellationToken = default)
    {
        if (unassignedOnly && assignedUserId.HasValue)
        {
            throw new ArgumentException("Filter by an assigned user or by unassigned tickets, not both.");
        }

        IQueryable<Ticket> query = _db.Tickets
            .AsNoTracking()
            .Include(t => t.AssignedUser);

        // Filters are applied in the database, not in memory.
        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        if (assignedUserId.HasValue)
        {
            query = query.Where(t => t.AssignedUserId == assignedUserId.Value);
        }
        else if (unassignedOnly)
        {
            query = query.Where(t => t.AssignedUserId == null);
        }

        var tickets = await query
            .OrderByDescending(t => t.UpdatedAt)
            .ThenByDescending(t => t.Id)
            .ToListAsync(cancellationToken);

        return tickets.Select(MapToResponse).ToList();
    }

    public async Task<TicketResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var ticket = await _db.Tickets
            .AsNoTracking()
            .Include(t => t.AssignedUser)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        return ticket is null ? null : MapToResponse(ticket);
    }

    public async Task<TicketResponse> CreateAsync(CreateTicketRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAssignedUserExistsAsync(request.AssignedUserId, cancellationToken);

        var ticket = new Ticket
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Status = TicketStatus.Open,
            AssignedUserId = request.AssignedUserId
        };

        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created Ticket {TicketId} with AssignedUserId {AssignedUserId}",
            ticket.Id, ticket.AssignedUserId);

        return (await GetByIdAsync(ticket.Id, cancellationToken))!;
    }

    public async Task<TicketResponse?> UpdateAsync(int id, UpdateTicketRequest request, CancellationToken cancellationToken = default)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (ticket is null)
        {
            return null;
        }

        var status = request.Status ?? throw new ArgumentException("Status is required.");

        ticket.Title = request.Title.Trim();
        ticket.Description = request.Description.Trim();
        ticket.Status = status;

        // UpdatedAt is stamped by AppDbContext.SaveChangesAsync, so no caller can forget it.
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated Ticket {TicketId}: Status={Status}", ticket.Id, ticket.Status);

        return await GetByIdAsync(ticket.Id, cancellationToken);
    }

    public async Task<TicketResponse?> AssignAsync(int id, int? assignedUserId, CancellationToken cancellationToken = default)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (ticket is null)
        {
            return null;
        }

        await ValidateAssignedUserExistsAsync(assignedUserId, cancellationToken);

        ticket.AssignedUserId = assignedUserId;
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Assigned Ticket {TicketId} to User {AssignedUserId}", ticket.Id, assignedUserId);

        return await GetByIdAsync(ticket.Id, cancellationToken);
    }

    private async Task ValidateAssignedUserExistsAsync(int? assignedUserId, CancellationToken cancellationToken)
    {
        if (!assignedUserId.HasValue)
        {
            return;
        }

        var exists = await _db.Users.AnyAsync(u => u.Id == assignedUserId.Value, cancellationToken);
        if (!exists)
        {
            _logger.LogWarning("Rejected ticket operation: User {AssignedUserId} does not exist", assignedUserId.Value);
            throw new ArgumentException($"Assigned user with ID {assignedUserId.Value} was not found.");
        }
    }

    private static TicketResponse MapToResponse(Ticket ticket) => new(
        ticket.Id,
        ticket.Title,
        ticket.Description,
        ticket.Status,
        ticket.CreatedAt,
        ticket.UpdatedAt,
        ticket.AssignedUserId,
        ticket.AssignedUser?.Name);
}
