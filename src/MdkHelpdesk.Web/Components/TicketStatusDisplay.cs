using MdkHelpdesk.Domain.Enums;

namespace MdkHelpdesk.Web.Components;

/// <summary>
/// Shared presentation helpers so every page shows ticket statuses the same way.
/// </summary>
public static class TicketStatusDisplay
{
    public static string Label(TicketStatus status) => status switch
    {
        TicketStatus.InProgress => "In Progress",
        _ => status.ToString()
    };

    public static string BadgeClass(TicketStatus status) => status switch
    {
        TicketStatus.Open => "bg-danger",
        TicketStatus.InProgress => "bg-warning text-dark",
        TicketStatus.Closed => "bg-success",
        _ => "bg-secondary"
    };
}
