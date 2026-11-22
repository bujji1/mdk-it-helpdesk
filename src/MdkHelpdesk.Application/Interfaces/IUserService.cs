using MdkHelpdesk.Application.DTOs;

namespace MdkHelpdesk.Application.Interfaces;

/// <summary>
/// Defines read operations for helpdesk users, used when assigning tickets.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Returns all helpdesk users ordered alphabetically by name.
    /// </summary>
    Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default);
}
