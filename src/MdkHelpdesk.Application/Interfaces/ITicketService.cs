using MdkHelpdesk.Application.DTOs;

namespace MdkHelpdesk.Application.Interfaces;

/// <summary>
/// Defines core ticket management operations shared by both the Blazor UI and REST API.
/// </summary>
public interface ITicketService
{
    /// <summary>
    /// Returns all tickets, most recently updated first.
    /// </summary>
    Task<IReadOnlyList<TicketResponse>> GetTicketsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a single ticket, or null when no ticket has the given ID.
    /// </summary>
    Task<TicketResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new ticket in the Open state.
    /// </summary>
    /// <exception cref="ArgumentException">The assigned user does not exist.</exception>
    Task<TicketResponse> CreateAsync(CreateTicketRequest request, CancellationToken cancellationToken = default);
}
