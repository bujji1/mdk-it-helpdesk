namespace MdkHelpdesk.Domain.Entities;

/// <summary>
/// Represents an IT helpdesk team member who can be assigned support tickets.
/// </summary>
public class User
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Tickets currently assigned to this user.
    /// </summary>
    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();
}
