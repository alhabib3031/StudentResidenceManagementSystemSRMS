using Microsoft.EntityFrameworkCore;
using SRMS.Infrastructure;
using SRMS.Infrastructure.Repositories;
using Xunit;
using FluentAssertions;
using Moq;
using SRMS.Domain.Residences;
using Microsoft.AspNetCore.Http;

namespace SRMS.Testing.Infrastructure.Repositories;

public class GenericRepositoryResidenceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly GenericRepository<Residence> _repository;

    public GenericRepositoryResidenceTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(new DefaultHttpContext());

        var contextFactoryMock = new Mock<IDbContextFactory<ApplicationDbContext>>();
        contextFactoryMock.Setup(f => f.CreateDbContext())
            .Returns(() => new ApplicationDbContext(_options, _httpContextAccessorMock.Object));
        contextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new ApplicationDbContext(_options, _httpContextAccessorMock.Object));
        
        _repository = new GenericRepository<Residence>(contextFactoryMock.Object);
    }
    
    private ApplicationDbContext CreateContext() => new ApplicationDbContext(_options, _httpContextAccessorMock.Object);

    [Fact]
    public async Task CreateAsync_Should_AddResidenceToDatabase()
    {
        // Arrange
        var residence = new Residence 
        { 
            Name = "Al-Wahat Residence",
            Description = "A modern residence.",
            MaxRoomsCount = 100
        };

        // Act
        var createdResidence = await _repository.CreateAsync(residence);
        
        await using var context = CreateContext();
        var residenceInDb = await context.Residences.FindAsync(createdResidence.Id);

        // Assert
        residenceInDb.Should().NotBeNull();
        residenceInDb!.Name.Should().Be("Al-Wahat Residence");
        residenceInDb.Id.Should().Be(createdResidence.Id);
        residenceInDb.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_Should_ModifyResidenceInDatabase()
    {
        // Arrange
        var residence = await _repository.CreateAsync(new Residence { Name = "Old Name" });
        
        // No need to detach, as the update will use a new context.
        var residenceToUpdate = await _repository.GetByIdAsync(residence.Id);
        residenceToUpdate!.Name = "New Name";
        residenceToUpdate.HasGym = true;

        // Act
        await _repository.UpdateAsync(residenceToUpdate);
        
        await using var context = CreateContext();
        var updatedResidenceFromDb = await context.Residences.FindAsync(residence.Id);

        // Assert
        updatedResidenceFromDb.Should().NotBeNull();
        updatedResidenceFromDb!.Name.Should().Be("New Name");
        updatedResidenceFromDb.HasGym.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_Should_SoftDeleteResidence()
    {
        // Arrange
        var residence = await _repository.CreateAsync(new Residence { Name = "To Be Deleted" });

        // Act
        var result = await _repository.DeleteAsync(residence.Id);
        var residenceAfterDelete = await _repository.GetByIdAsync(residence.Id);
        
        await using var context = CreateContext();
        var residenceInDb = await context.Residences.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == residence.Id);

        // Assert
        result.Should().BeTrue();
        residenceAfterDelete.Should().BeNull(); // Should be null because of the soft-delete filter
        residenceInDb.Should().NotBeNull();
        residenceInDb!.IsDeleted.Should().BeTrue();
    }
}
