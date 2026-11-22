using MdkHelpdesk.Application.DTOs;
using MdkHelpdesk.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MdkHelpdesk.Application.Services;

/// <summary>
/// Provides read-only access to helpdesk users for assignment dropdowns.
/// </summary>
public sealed class UserService : IUserService
{
    private readonly IAppDbContext _db;

    public UserService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Users
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .Select(u => new UserResponse(u.Id, u.Name, u.Email))
            .ToListAsync(cancellationToken);
    }
}
