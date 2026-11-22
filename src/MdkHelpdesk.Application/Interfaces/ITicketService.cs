using MdkHelpdesk.Application.DTOs;
using MdkHelpdesk.Domain.Enums;

namespace MdkHelpdesk.Application.Interfaces;

/// <summary>
/// Defines core ticket management operations shared by both the Blazor UI and REST API.
/// </summary>
public interface ITicketService
{
    /// <summary>
    /// Returns tickets, most recently updated first, optionally filtered by status and by assignment
    /// (either a specific user, or only tickets with no assignee).
    /// </summary>
    /// <exception cref="ArgumentException">Both a user and "unassigned only" were requested.</exception>
    Task<IReadOnlyList<TicketResponse>> GetTicketsAsync(
        TicketStatus? status = null,
        int? assignedUserId = null,
        bool unassignedOnly = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a single ticket, or null when no ticket has the given ID.
    /// </summary>
    Task<TicketResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new ticket in the Open state.
    /// </summary>
    /// <exception cref="ArgumentException">The assigned user does not exist.</exception>
    Task<TicketResponse> CreateAsync(CreateTicketRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a ticket's title, description and status. Returns null when the ticket does not exist.
    /// </summary>
    Task<TicketResponse?> UpdateAsync(int id, UpdateTicketRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a ticket to a user, or unassigns it when <paramref name="assignedUserId"/> is null.
    /// Returns null when the ticket does not exist.
    /// </summary>
    /// <exception cref="ArgumentException">The assigned user does not exist.</exception>
    Task<TicketResponse?> AssignAsync(int id, int? assignedUserId, CancellationToken cancellationToken = default);
}
