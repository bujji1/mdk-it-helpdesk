using MdkHelpdesk.Domain.Enums;

namespace MdkHelpdesk.Domain.Entities;

/// <summary>
/// Represents an internal IT support ticket tracked by the helpdesk team.
/// </summary>
public class Ticket
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public TicketStatus Status { get; set; } = TicketStatus.Open;

    /// <summary>
    /// UTC timestamp when the ticket was created. Managed automatically by the data layer.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// UTC timestamp when the ticket was last modified. Managed automatically by the data layer.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Optional foreign key to the assigned helpdesk user (null when unassigned for triage).
    /// </summary>
    public int? AssignedUserId { get; set; }

    public User? AssignedUser { get; set; }
}
