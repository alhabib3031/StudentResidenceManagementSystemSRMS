using Microsoft.EntityFrameworkCore;
using SRMS.Infrastructure;
using SRMS.Infrastructure.Configurations.Services;
using Xunit;
using FluentAssertions;
using Moq;
using SRMS.Domain.Colleges;
using SRMS.Domain.Colleges.Enums;
using Microsoft.AspNetCore.Http;

namespace SRMS.Testing.Infrastructure.Services;

public class CollegeServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly CollegeService _collegeService;

    public CollegeServiceTests()
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

        _collegeService = new CollegeService(contextFactoryMock.Object);
    }
    
    private ApplicationDbContext CreateContext() => new ApplicationDbContext(_options, _httpContextAccessorMock.Object);

    [Fact]
    public async Task CreateCollegeAsync_Should_AddCollegeToDatabase()
    {
        // Arrange
        var college = new College { Name = "Engineering", StudySystem = StudySystem.Semesters };

        // Act
        var result = await _collegeService.CreateCollegeAsync(college);
        
        // Assert
        await using var context = CreateContext();
        var collegeInDb = await context.Colleges.FirstOrDefaultAsync(c => c.Name == "Engineering");

        result.Should().BeTrue();
        collegeInDb.Should().NotBeNull();
        collegeInDb!.Name.Should().Be("Engineering");
    }

    [Fact]
    public async Task GetCollegeByIdAsync_Should_ReturnCorrectCollege()
    {
        // Arrange
        var college = new College { Name = "Science" };
        await _collegeService.CreateCollegeAsync(college);

        // Act
        var result = await _collegeService.GetCollegeByIdAsync(college.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(college.Id);
        result.Name.Should().Be("Science");
    }

    [Fact]
    public async Task UpdateCollegeAsync_Should_SaveChangesToDatabase()
    {
        // Arrange
        var college = new College { Name = "Old Name" };
        await _collegeService.CreateCollegeAsync(college);
        
        var collegeToUpdate = await _collegeService.GetCollegeByIdAsync(college.Id);
        collegeToUpdate!.Name = "New Name";

        // Act
        var result = await _collegeService.UpdateCollegeAsync(collegeToUpdate);
        
        await using var context = CreateContext();
        var collegeInDb = await context.Colleges.FindAsync(college.Id);

        // Assert
        result.Should().BeTrue();
        collegeInDb.Should().NotBeNull();
        collegeInDb!.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task DeleteCollegeAsync_Should_RemoveCollegeFromDatabase()
    {
        // Arrange
        var college = new College { Name = "To Be Deleted" };
        await _collegeService.CreateCollegeAsync(college);
        var collegeId = college.Id;

        // Act
        var result = await _collegeService.DeleteCollegeAsync(collegeId);
        
        await using var context = CreateContext();
        var collegeInDb = await context.Colleges.FindAsync(collegeId);

        // Assert
        result.Should().BeTrue();
        collegeInDb.Should().BeNull(); // Hard delete
    }

    [Fact]
    public async Task CreateMajorAsync_Should_AddMajorToDatabase()
    {
        // Arrange
        var college = new College { Name = "Arts" };
        await _collegeService.CreateCollegeAsync(college);
        var major = new Major { Name = "History", CollegeId = college.Id };

        // Act
        var result = await _collegeService.CreateMajorAsync(major);
        
        await using var context = CreateContext();
        var majorInDb = await context.Majors.FirstOrDefaultAsync(m => m.Name == "History");

        // Assert
        result.Should().BeTrue();
        majorInDb.Should().NotBeNull();
        majorInDb!.CollegeId.Should().Be(college.Id);
    }
}
