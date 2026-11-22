using MdkHelpdesk.Application.Services;
using MdkHelpdesk.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MdkHelpdesk.UnitTests;

public sealed class UserServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();

        _sut = new UserService(_db);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsSeededUsersOrderedByName()
    {
        var users = await _sut.GetAllAsync();

        Assert.Equal(
            new[] { "Alice Johnson", "Bob Martinez", "Priya Patel" },
            users.Select(u => u.Name));
        Assert.All(users, u => Assert.False(string.IsNullOrWhiteSpace(u.Email)));
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
