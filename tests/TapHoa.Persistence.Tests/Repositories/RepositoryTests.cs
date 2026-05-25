using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Persistence.Data;
using TapHoa.Persistence.Repositories;

namespace TapHoa.Persistence.Tests.Repositories;

public class RepositoryTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly Repository<User> _repo;

    public RepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _repo = new Repository<User>(_db);
    }

    [Fact]
    public async Task AddAsync_NewUser_CanBeRetrievedById()
    {
        // Arrange
        var user = new User { FullName = "Nguyen Van A", Email = "a@a.com", PasswordHash = "hash" };

        // Act
        await _repo.AddAsync(user);
        await _repo.SaveChangesAsync();

        var found = await _repo.GetByIdAsync(user.Id);

        // Assert
        found.Should().NotBeNull();
        found!.Email.Should().Be("a@a.com");
    }

    [Fact]
    public async Task FindAsync_ByEmail_ReturnsCorrectUser()
    {
        // Arrange
        var user = new User { FullName = "B", Email = "b@b.com", PasswordHash = "hash" };
        await _repo.AddAsync(user);
        await _repo.SaveChangesAsync();

        // Act
        var found = await _repo.FindAsync(u => u.Email == "b@b.com");

        // Assert
        found.Should().NotBeNull();
        found!.FullName.Should().Be("B");
    }

    [Fact]
    public async Task AnyAsync_ExistingEmail_ReturnsTrue()
    {
        var user = new User { FullName = "C", Email = "c@c.com", PasswordHash = "hash" };
        await _repo.AddAsync(user);
        await _repo.SaveChangesAsync();

        var exists = await _repo.AnyAsync(u => u.Email == "c@c.com");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task AnyAsync_NonExistingEmail_ReturnsFalse()
    {
        var exists = await _repo.AnyAsync(u => u.Email == "notexist@x.com");
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task Remove_ExistingUser_CannotBeFoundAfterSave()
    {
        var user = new User { FullName = "D", Email = "d@d.com", PasswordHash = "hash" };
        await _repo.AddAsync(user);
        await _repo.SaveChangesAsync();

        _repo.Remove(user);
        await _repo.SaveChangesAsync();

        var found = await _repo.GetByIdAsync(user.Id);
        found.Should().BeNull();
    }

    public void Dispose() => _db.Dispose();
}
