using Microsoft.EntityFrameworkCore;
using SRMS.Infrastructure;
using SRMS.Infrastructure.Configurations.Services;
using Xunit;
using FluentAssertions;
using Moq;
using SRMS.Domain.AuditLogs;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SRMS.Testing.Infrastructure.Services;

public class AuditServiceTests : IDisposable
{
    private readonly AuditService _auditService;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly ApplicationDbContext _context;

    public AuditServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _context = new ApplicationDbContext(options, httpContextAccessorMock.Object);

        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        var optionsAccessorMock = new Mock<IOptions<IdentityOptions>>();
        var passwordHasherMock = new Mock<IPasswordHasher<ApplicationUser>>();
        var userValidators = new List<IUserValidator<ApplicationUser>>();
        var passwordValidators = new List<IPasswordValidator<ApplicationUser>>();
        var keyNormalizerMock = new Mock<ILookupNormalizer>();
        var errorsMock = new Mock<IdentityErrorDescriber>();
        var servicesMock = new Mock<IServiceProvider>();
        var loggerMock = new Mock<ILogger<UserManager<ApplicationUser>>>();

        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, 
            optionsAccessorMock.Object, 
            passwordHasherMock.Object, 
            userValidators, 
            passwordValidators, 
            keyNormalizerMock.Object, 
            errorsMock.Object, 
            servicesMock.Object, 
            loggerMock.Object);

        _auditService = new AuditService(_context, _httpContextAccessorMock.Object, _userManagerMock.Object);
    }

    [Fact]
    public async Task LogAsync_Should_CreateAuditLogInDatabase()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Email = "test@example.com" };
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
        
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(httpContext);
        _userManagerMock.Setup(um => um.GetUserAsync(claimsPrincipal)).ReturnsAsync(user);

        // Act
        await _auditService.LogAsync(
            action: AuditAction.Login,
            entityName: "Authentication",
            additionalInfo: "Test login"
        );

        var auditLog = await _context.AuditLogs.FirstOrDefaultAsync();

        // Assert
        auditLog.Should().NotBeNull();
        auditLog!.Action.Should().Be("Login Authentication");
        auditLog.UserName.Should().Be("test@example.com");
        auditLog.UserId.Should().Be(userId.ToString());
        auditLog.AdditionalInfo.Should().Be("Test login");
    }

    [Fact]
    public async Task LogCrudAsync_Should_SerializeEntitiesAndLogCorrectly()
    {
        // Arrange
        var oldEntity = new { Name = "Old", Value = 1 };
        var newEntity = new { Name = "New", Value = 2 };

        // Act
        await _auditService.LogCrudAsync(
            action: AuditAction.Update,
            oldEntity: oldEntity,
            newEntity: newEntity
        );

        var auditLog = await _context.AuditLogs.FirstOrDefaultAsync();

        // Assert
        auditLog.Should().NotBeNull();
        auditLog!.Action.Should().StartWith("Update");
        auditLog.OldValues.Should().Contain("\"Name\":\"Old\"");
        auditLog.OldValues.Should().Contain("\"Value\":1");
        auditLog.NewValues.Should().Contain("\"Name\":\"New\"");
        auditLog.NewValues.Should().Contain("\"Value\":2");
    }

    [Fact]
    public async Task GetUserLogsAsync_Should_ReturnOnlyLogsForSpecifiedUser()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        _context.AuditLogs.Add(new AuditLog { Id = Guid.NewGuid(), UserId = userId1.ToString(), Timestamp = DateTime.UtcNow, Action = "Action1" });
        _context.AuditLogs.Add(new AuditLog { Id = Guid.NewGuid(), UserId = userId2.ToString(), Timestamp = DateTime.UtcNow, Action = "Action2" });
        _context.AuditLogs.Add(new AuditLog { Id = Guid.NewGuid(), UserId = userId1.ToString(), Timestamp = DateTime.UtcNow, Action = "Action3" });
        await _context.SaveChangesAsync();

        // Act
        var user1Logs = await _auditService.GetUserLogsAsync(userId1);
        var user2Logs = await _auditService.GetUserLogsAsync(userId2);

        // Assert
        user1Logs.Should().HaveCount(2);
        user1Logs.Should().OnlyContain(l => l.UserId == userId1.ToString());
        
        user2Logs.Should().HaveCount(1);
        user2Logs.Should().OnlyContain(l => l.UserId == userId2.ToString());
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
