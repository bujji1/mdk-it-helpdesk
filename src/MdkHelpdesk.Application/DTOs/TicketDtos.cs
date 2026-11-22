using MdkHelpdesk.Domain.Enums;

namespace MdkHelpdesk.Application.DTOs;

/// <summary>
/// Public representation of a support ticket returned by APIs and UI services.
/// </summary>
public sealed record TicketResponse(
    int Id,
    string Title,
    string Description,
    TicketStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int? AssignedUserId,
    string? AssignedUserName);
