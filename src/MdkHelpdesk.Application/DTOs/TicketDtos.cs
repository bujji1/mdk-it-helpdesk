using System.ComponentModel.DataAnnotations;
using MdkHelpdesk.Domain.Enums;

namespace MdkHelpdesk.Application.DTOs;

/// <summary>
/// Payload for creating a new support ticket.
/// </summary>
public sealed class CreateTicketRequest
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 150 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Optional user ID to assign immediately upon creation. Leave empty to create the ticket unassigned.
    /// </summary>
    public int? AssignedUserId { get; set; }
}

/// <summary>
/// Payload for updating an existing support ticket's details and status.
/// </summary>
public sealed class UpdateTicketRequest
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 150 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string Description { get; set; } = string.Empty;

    [EnumDataType(typeof(TicketStatus), ErrorMessage = "Status must be Open, InProgress or Closed.")]
    public TicketStatus Status { get; set; }
}

/// <summary>
/// Payload for assigning or unassigning a ticket.
/// </summary>
public sealed class AssignTicketRequest
{
    /// <summary>
    /// Target user ID, or null to unassign the ticket.
    /// </summary>
    public int? AssignedUserId { get; set; }
}

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
