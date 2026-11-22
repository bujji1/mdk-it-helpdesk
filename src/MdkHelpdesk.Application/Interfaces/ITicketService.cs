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
}
