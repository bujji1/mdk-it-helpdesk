using MdkHelpdesk.Application.DTOs;
using MdkHelpdesk.Application.Interfaces;
using MdkHelpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MdkHelpdesk.Application.Services;

/// <summary>
/// Implements core helpdesk ticket workflows on top of EF Core persistence.
/// </summary>
public sealed class TicketService : ITicketService
{
    private readonly IAppDbContext _db;

    public TicketService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<TicketResponse>> GetTicketsAsync(CancellationToken cancellationToken = default)
    {
        var tickets = await _db.Tickets
            .AsNoTracking()
            .Include(t => t.AssignedUser)
            .OrderByDescending(t => t.UpdatedAt)
            .ThenByDescending(t => t.Id)
            .ToListAsync(cancellationToken);

        return tickets.Select(MapToResponse).ToList();
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
