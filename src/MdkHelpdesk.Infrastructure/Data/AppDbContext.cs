using MdkHelpdesk.Application.Interfaces;
using MdkHelpdesk.Domain.Entities;
using MdkHelpdesk.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MdkHelpdesk.Infrastructure.Data;

/// <summary>
/// Entity Framework Core DbContext configuring relational mappings, seed data, and automatic UTC timestamps.
/// </summary>
public sealed class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Name).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(u => u.Email).IsUnique();

            entity.HasData(
                new User { Id = 1, Name = "Alice Johnson", Email = "alice.johnson@mdklegal.com" },
                new User { Id = 2, Name = "Bob Martinez", Email = "bob.martinez@mdklegal.com" },
                new User { Id = 3, Name = "Priya Patel", Email = "priya.patel@mdklegal.com" }
            );
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).IsRequired().HasMaxLength(150);
            entity.Property(t => t.Description).IsRequired().HasMaxLength(2000);

            // Store status as readable string in SQL for easier DB inspection and debugging.
            entity.Property(t => t.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(t => t.CreatedAt).IsRequired();
            entity.Property(t => t.UpdatedAt).IsRequired();

            entity.HasOne(t => t.AssignedUser)
                .WithMany(u => u.AssignedTickets)
                .HasForeignKey(t => t.AssignedUserId)
                .OnDelete(DeleteBehavior.SetNull);

            var seedTime = new DateTime(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc);
            entity.HasData(
                new Ticket
                {
                    Id = 1,
                    Title = "VPN connection drops intermittently",
                    Description = "Remote user reports VPN disconnecting every 15 minutes on Windows 11.",
                    Status = TicketStatus.Open,
                    AssignedUserId = 1,
                    CreatedAt = seedTime,
                    UpdatedAt = seedTime
                },
                new Ticket
                {
                    Id = 2,
                    Title = "Request Outlook shared mailbox access",
                    Description = "Grant read/send-as permissions for legal-intake@mdklegal.com.",
                    Status = TicketStatus.InProgress,
                    AssignedUserId = 2,
                    CreatedAt = seedTime,
                    UpdatedAt = seedTime
                },
                new Ticket
                {
                    Id = 3,
                    Title = "Conference room projector HDMI issue",
                    Description = "3rd floor conference room display shows 'No Signal' when docked.",
                    Status = TicketStatus.Closed,
                    AssignedUserId = 3,
                    CreatedAt = seedTime,
                    UpdatedAt = seedTime
                }
            );
        });
    }

    /// <summary>
    /// Automatically enforces UTC CreatedAt and UpdatedAt timestamps on every insert/update.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<Ticket>())
        {
            if (entry.State == EntityState.Added)
            {
                // Preserve explicit seed timestamps only if already set on initial seeding; otherwise stamp UTC now.
                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = utcNow;
                }
                if (entry.Entity.UpdatedAt == default)
                {
                    entry.Entity.UpdatedAt = utcNow;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = utcNow;
                entry.Property(t => t.CreatedAt).IsModified = false;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
