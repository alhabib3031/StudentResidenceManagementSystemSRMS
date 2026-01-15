using Microsoft.EntityFrameworkCore;
using SRMS.Domain.Students;
using SRMS.Infrastructure.Repositories;
using Xunit;
using FluentAssertions;
using SRMS.Infrastructure;
using Moq;
using Microsoft.AspNetCore.Http;

namespace SRMS.Testing.Infrastructure.Repositories;

public class GenericRepositoryTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly GenericRepository<Student> _repository;

    public GenericRepositoryTests()
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
        
        _repository = new GenericRepository<Student>(contextFactoryMock.Object);
    }
    
    private ApplicationDbContext CreateContext() => new ApplicationDbContext(_options, _httpContextAccessorMock.Object);

    private async Task<Student> SeedStudentAsync()
    {
        var student = new Student { FirstName = "Initial", LastName = "Student" };
        return await _repository.CreateAsync(student);
    }

    [Fact]
    public async Task CreateAsync_Should_AddEntityToDatabase_And_SetAuditProperties()
    {
        // Arrange
        var student = new Student { FirstName = "Test", LastName = "Student" };

        // Act
        var createdStudent = await _repository.CreateAsync(student);
        
        await using var context = CreateContext();
        var studentInDb = await context.Students.FindAsync(createdStudent.Id);

        // Assert
        studentInDb.Should().NotBeNull();
        studentInDb!.FirstName.Should().Be("Test");
        createdStudent.Id.Should().NotBe(Guid.Empty);
        createdStudent.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        createdStudent.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        createdStudent.IsActive.Should().BeTrue();
        createdStudent.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnCorrectEntity_WhenEntityExists()
    {
        // Arrange
        var seededStudent = await SeedStudentAsync();

        // Act
        var foundStudent = await _repository.GetByIdAsync(seededStudent.Id);

        // Assert
        foundStudent.Should().NotBeNull();
        foundStudent!.Id.Should().Be(seededStudent.Id);
        foundStudent.FirstName.Should().Be("Initial");
    }
    
    [Fact]
    public async Task GetByIdAsync_Should_ReturnNull_WhenEntityIsSoftDeleted()
    {
        // Arrange
        var seededStudent = await SeedStudentAsync();
        await _repository.DeleteAsync(seededStudent.Id);

        // Act
        var foundStudent = await _repository.GetByIdAsync(seededStudent.Id);

        // Assert
        foundStudent.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_Should_ModifyEntityInDatabase()
    {
        // Arrange
        var seededStudent = await SeedStudentAsync();
        var initialCreationTime = seededStudent.CreatedAt;
        
        var studentToUpdate = await _repository.GetByIdAsync(seededStudent.Id);
        studentToUpdate!.FirstName = "Updated";
        
        // Act
        await _repository.UpdateAsync(studentToUpdate);
        
        await using var context = CreateContext();
        var updatedStudentFromDb = await context.Students.FindAsync(seededStudent.Id);

        // Assert
        updatedStudentFromDb.Should().NotBeNull();
        updatedStudentFromDb!.FirstName.Should().Be("Updated");
        updatedStudentFromDb.CreatedAt.Should().Be(initialCreationTime);
        updatedStudentFromDb.UpdatedAt.Should().BeAfter(initialCreationTime);
    }

    [Fact]
    public async Task DeleteAsync_Should_SoftDeleteEntity()
    {
        // Arrange
        var seededStudent = await SeedStudentAsync();

        // Act
        var result = await _repository.DeleteAsync(seededStudent.Id);
        var studentAfterDelete = await _repository.GetByIdAsync(seededStudent.Id);
        
        await using var context = CreateContext();
        var studentInDb = await context.Students.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == seededStudent.Id);
        
        // Assert
        result.Should().BeTrue();
        studentAfterDelete.Should().BeNull();
        studentInDb.Should().NotBeNull();
        studentInDb!.IsDeleted.Should().BeTrue();
        studentInDb.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        studentInDb.IsActive.Should().BeFalse();
    }
    
    [Fact]
    public async Task GetAllAsync_Should_ReturnOnlyNonDeletedEntities()
    {
        // Arrange
        await SeedStudentAsync(); // Student 1
        var student2 = await SeedStudentAsync(); // Student 2
        await _repository.DeleteAsync(student2.Id); // Soft delete student 2

        // Act
        var allStudents = await _repository.GetAllAsync();

        // Assert
        allStudents.Should().NotBeNull();
        allStudents.Should().HaveCount(1);
        allStudents.First().Id.Should().NotBe(student2.Id);
    }
}
