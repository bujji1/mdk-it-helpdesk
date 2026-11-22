using MdkHelpdesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MdkHelpdesk.Application.Interfaces;

/// <summary>
/// Abstraction over the Entity Framework Core DbContext so the Application layer remains decoupled from Infrastructure.
/// </summary>
public interface IAppDbContext
{
    DbSet<Ticket> Tickets { get; }
    DbSet<User> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
