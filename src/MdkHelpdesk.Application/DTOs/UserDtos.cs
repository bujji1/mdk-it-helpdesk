namespace MdkHelpdesk.Application.DTOs;

/// <summary>
/// Public representation of a helpdesk user.
/// </summary>
public sealed record UserResponse(int Id, string Name, string Email);
