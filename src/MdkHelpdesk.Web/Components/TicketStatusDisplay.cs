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

    /// <summary>
    /// CSS class for the status badge; the colours are defined in wwwroot/app.css.
    /// </summary>
    public static string BadgeClass(TicketStatus status) => status switch
    {
        TicketStatus.Open => "status-open",
        TicketStatus.InProgress => "status-progress",
        TicketStatus.Closed => "status-closed",
        _ => "bg-secondary"
    };
}
