using Microsoft.EntityFrameworkCore;
using SRMS.Infrastructure;
using SRMS.Infrastructure.Repositories;
using SRMS.Infrastructure.Configurations.Services;
using Xunit;
using FluentAssertions;
using Moq;
using SRMS.Domain.Notifications;
using SRMS.Application.Notifications.DTOs;
using SRMS.Application.Notifications.Interfaces;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SRMS.Testing.Infrastructure.Services;

public class NotificationServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly NotificationService _notificationService;
    private readonly GenericRepository<Notification> _notificationRepository;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ISMSService> _smsServiceMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;

    public NotificationServiceTests()
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

        _notificationRepository = new GenericRepository<Notification>(contextFactoryMock.Object);
        _emailServiceMock = new Mock<IEmailService>();
        _smsServiceMock = new Mock<ISMSService>();
        _auditServiceMock = new Mock<IAuditService>();

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

        _notificationService = new NotificationService(
            _notificationRepository,
            _emailServiceMock.Object,
            _smsServiceMock.Object,
            _auditServiceMock.Object,
            _userManagerMock.Object
        );
    }
    
    private ApplicationDbContext CreateContext() => new ApplicationDbContext(_options, _httpContextAccessorMock.Object);

    [Fact]
    public async Task SendNotificationAsync_Should_CreateNotificationInDb_And_CallEmailService()
    {
        // Arrange
        var dto = new CreateNotificationDto
        {
            Title = "Test Email",
            Message = "Hello",
            UserEmail = "test@example.com",
            SendEmail = true,
            SendSMS = false
        };

        // Act
        var result = await _notificationService.SendNotificationAsync(dto);
        await Task.Delay(100); 
        
        await using var context = CreateContext();
        var notificationInDb = await context.Notifications.FirstOrDefaultAsync();

        // Assert
        result.Should().BeTrue();
        notificationInDb.Should().NotBeNull();
        notificationInDb!.Title.Should().Be("Test Email");
        notificationInDb.Status.Should().Be(Domain.Notifications.Enums.NotificationStatus.Sent);

        _emailServiceMock.Verify(s => s.SendEmailAsync("test@example.com", "Test Email", "Hello", true), Times.Once);
        _smsServiceMock.Verify(s => s.SendSMSAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendNotificationToRoleAsync_Should_AttemptToSendToAllUsersInRole()
    {
        // Arrange
        var roleName = "Manager";
        var users = new List<ApplicationUser>
        {
            new ApplicationUser { Id = Guid.NewGuid(), Email = "manager1@example.com" },
            new ApplicationUser { Id = Guid.NewGuid(), Email = "manager2@example.com" }
        };
        var dto = new CreateNotificationDto { Title = "For Managers", SendEmail = true };

        _userManagerMock.Setup(um => um.GetUsersInRoleAsync(roleName)).ReturnsAsync(users);

        // Act
        await _notificationService.SendNotificationToRoleAsync(roleName, dto);
        await Task.Delay(100); 

        // Assert
        _emailServiceMock.Verify(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Exactly(2));
        
        await using var context = CreateContext();
        var notificationsInDb = await context.Notifications.ToListAsync();
        notificationsInDb.Count.Should().Be(2);
    }

    [Fact]
    public async Task MarkAsReadAsync_Should_UpdateNotificationStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification = await _notificationRepository.CreateAsync(new Notification { UserId = userId, IsRead = false });

        // Act
        var result = await _notificationService.MarkAsReadAsync(notification.Id, userId);
        
        await using var context = CreateContext();
        var notificationInDb = await context.Notifications.FindAsync(notification.Id);

        // Assert
        result.Should().BeTrue();
        notificationInDb.Should().NotBeNull();
        notificationInDb!.IsRead.Should().BeTrue();
        notificationInDb.ReadAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        notificationInDb.Status.Should().Be(Domain.Notifications.Enums.NotificationStatus.Read);
    }
}
