using FluentAssertions;
using Xunit;
using Moq;
using AutoMapper;
using MediatR;
using UserProfileService.Application.Commands;
using UserProfileService.Application.Queries;
using UserProfileService.Application.Handlers;
using UserProfileService.Application.DTOs;
using UserProfileService.Application.Interfaces;
using UserProfileService.Domain.Aggregates;
using SharedKernel;

namespace UserService.Application.Tests.Handlers;

/// <summary>
/// Tests for user profile command handlers.
/// </summary>
public class UpdateProfileCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidInput_ShouldUpdateProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userProfileId = Guid.NewGuid();
        var userProfile = new UserProfile(userProfileId, userId, "user@example.com");

        var mockRepository = new Mock<IUserProfileRepository>();
        mockRepository.Setup(r => r.GetByIdAsync(userProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userProfile);
        mockRepository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var mockMapper = new Mock<IMapper>();
        var expectedDto = new UserProfileDto
        {
            Id = userProfileId,
            UserId = userId,
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe"
        };
        mockMapper.Setup(m => m.Map<UserProfileDto>(It.IsAny<UserProfile>()))
            .Returns(expectedDto);

        var handler = new UpdateProfileCommandHandler(mockRepository.Object, mockMapper.Object);
        var command = new UpdateProfileCommand(userProfileId, "John", "Doe", null, null, null, null, null, null, null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonexistentProfile_ShouldThrowNotFoundException()
    {
        // Arrange
        var userProfileId = Guid.NewGuid();
        var mockRepository = new Mock<IUserProfileRepository>();
        mockRepository.Setup(r => r.GetByIdAsync(userProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfile?)null);

        var mockMapper = new Mock<IMapper>();
        var handler = new UpdateProfileCommandHandler(mockRepository.Object, mockMapper.Object);
        var command = new UpdateProfileCommand(userProfileId, "John", "Doe", null, null, null, null, null, null, null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithAllProfileFields_ShouldUpdateAll()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userProfileId = Guid.NewGuid();
        var userProfile = new UserProfile(userProfileId, userId, "user@example.com");

        var mockRepository = new Mock<IUserProfileRepository>();
        mockRepository.Setup(r => r.GetByIdAsync(userProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userProfile);
        mockRepository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var mockMapper = new Mock<IMapper>();
        mockMapper.Setup(m => m.Map<UserProfileDto>(It.IsAny<UserProfile>()))
            .Returns(new UserProfileDto());

        var handler = new UpdateProfileCommandHandler(mockRepository.Object, mockMapper.Object);
        var command = new UpdateProfileCommand(
            userProfileId,
            "John", "Doe", "I love pets",
            new DateTime(1990, 5, 15),
            "+1234567890",
            "123 Main St",
            "Springfield",
            "USA",
            "https://picture.url");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        userProfile.Profile.FirstName.Should().Be("John");
        userProfile.Profile.LastName.Should().Be("Doe");
        userProfile.Profile.Bio.Should().Be("I love pets");
        userProfile.Profile.DateOfBirth.Should().Be(new DateTime(1990, 5, 15));
        userProfile.Profile.PhoneNumber.Should().Be("+1234567890");
    }
}

/// <summary>
/// Tests for notification preferences command handler.
/// </summary>
public class UpdateNotificationPreferencesCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidInput_ShouldUpdatePreferences()
    {
        // Arrange
        var userProfileId = Guid.NewGuid();
        var userProfile = new UserProfile(userProfileId, Guid.NewGuid(), "user@example.com");

        var mockRepository = new Mock<IUserProfileRepository>();
        mockRepository.Setup(r => r.GetByIdAsync(userProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userProfile);
        mockRepository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var mockMapper = new Mock<IMapper>();
        var expectedDto = new NotificationPreferencesDto
        {
            EmailNotifications = false,
            PushNotifications = true,
            SmsNotifications = true
        };
        mockMapper.Setup(m => m.Map<NotificationPreferencesDto>(It.IsAny<object>()))
            .Returns(expectedDto);

        var handler = new UpdateNotificationPreferencesCommandHandler(mockRepository.Object, mockMapper.Object);
        var command = new UpdateNotificationPreferencesCommand(userProfileId, false, true, true, false, true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.EmailNotifications.Should().BeFalse();
        result.PushNotifications.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonexistentProfile_ShouldThrowNotFoundException()
    {
        // Arrange
        var userProfileId = Guid.NewGuid();
        var mockRepository = new Mock<IUserProfileRepository>();
        mockRepository.Setup(r => r.GetByIdAsync(userProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfile?)null);

        var mockMapper = new Mock<IMapper>();
        var handler = new UpdateNotificationPreferencesCommandHandler(mockRepository.Object, mockMapper.Object);
        var command = new UpdateNotificationPreferencesCommand(userProfileId, true, true, true, true, true);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }
}

/// <summary>
/// Tests for language/timezone command handler.
/// </summary>
public class UpdateLanguageAndTimezoneCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidInput_ShouldUpdateLanguageAndTimezone()
    {
        // Arrange
        var userProfileId = Guid.NewGuid();
        var userProfile = new UserProfile(userProfileId, Guid.NewGuid(), "user@example.com");

        var mockRepository = new Mock<IUserProfileRepository>();
        mockRepository.Setup(r => r.GetByIdAsync(userProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userProfile);
        mockRepository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdateLanguageAndTimezoneCommandHandler(mockRepository.Object);
        var command = new UpdateLanguageAndTimezoneCommand(userProfileId, "es", "America/New_York");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        userProfile.Preferences.Language.Should().Be("es");
        userProfile.Preferences.Timezone.Should().Be("America/New_York");
    }

    [Fact]
    public async Task Handle_WithNonexistentProfile_ShouldThrowNotFoundException()
    {
        // Arrange
        var userProfileId = Guid.NewGuid();
        var mockRepository = new Mock<IUserProfileRepository>();
        mockRepository.Setup(r => r.GetByIdAsync(userProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfile?)null);

        var handler = new UpdateLanguageAndTimezoneCommandHandler(mockRepository.Object);
        var command = new UpdateLanguageAndTimezoneCommand(userProfileId, "fr", "Europe/Paris");

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }
}

/// <summary>
/// Tests for deactivate profile command handler.
/// </summary>
public class DeactivateProfileCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidProfile_ShouldDeactivate()
    {
        // Arrange
        var userProfileId = Guid.NewGuid();
        var userProfile = new UserProfile(userProfileId, Guid.NewGuid(), "user@example.com");
        userProfile.IsActive.Should().BeTrue();

        var mockRepository = new Mock<IUserProfileRepository>();
        mockRepository.Setup(r => r.GetByIdAsync(userProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userProfile);
        mockRepository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeactivateProfileCommandHandler(mockRepository.Object);
        var command = new DeactivateProfileCommand(userProfileId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        userProfile.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithNonexistentProfile_ShouldThrowNotFoundException()
    {
        // Arrange
        var userProfileId = Guid.NewGuid();
        var mockRepository = new Mock<IUserProfileRepository>();
        mockRepository.Setup(r => r.GetByIdAsync(userProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfile?)null);

        var handler = new DeactivateProfileCommandHandler(mockRepository.Object);
        var command = new DeactivateProfileCommand(userProfileId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }
}

/// <summary>
/// Tests for query handlers.
/// </summary>
public class UserProfileQueryHandlerTests
{
    [Fact]
    public async Task GetUserProfileQuery_WithValidId_ShouldReturnProfile()
    {
        // Arrange
        var userProfileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var userProfile = new UserProfile(userProfileId, userId, "user@example.com");

        var mockRepository = new Mock<IUserProfileRepository>();
        mockRepository.Setup(r => r.GetByIdAsync(userProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userProfile);

        var mockMapper = new Mock<IMapper>();
        var expectedDto = new UserProfileDto { Id = userProfileId, UserId = userId };
        mockMapper.Setup(m => m.Map<UserProfileDto>(userProfile))
            .Returns(expectedDto);

        var handler = new GetUserProfileQueryHandler(mockRepository.Object, mockMapper.Object);
        var query = new GetUserProfileQuery(userProfileId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userProfileId);
    }

    [Fact]
    public async Task GetUserProfileQuery_WithNonexistentId_ShouldThrowNotFoundException()
    {
        // Arrange
        var userProfileId = Guid.NewGuid();
        var mockRepository = new Mock<IUserProfileRepository>();
        mockRepository.Setup(r => r.GetByIdAsync(userProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfile?)null);

        var mockMapper = new Mock<IMapper>();
        var handler = new GetUserProfileQueryHandler(mockRepository.Object, mockMapper.Object);
        var query = new GetUserProfileQuery(userProfileId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task GetUserProfileByUserIdQuery_WithValidId_ShouldReturnProfile()
    {
        // Arrange
        var userProfileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var userProfile = new UserProfile(userProfileId, userId, "user@example.com");

        var mockRepository = new Mock<IUserProfileRepository>();
        mockRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userProfile);

        var mockMapper = new Mock<IMapper>();
        var expectedDto = new UserProfileDto { Id = userProfileId, UserId = userId };
        mockMapper.Setup(m => m.Map<UserProfileDto>(userProfile))
            .Returns(expectedDto);

        var handler = new GetUserProfileByUserIdQueryHandler(mockRepository.Object, mockMapper.Object);
        var query = new GetUserProfileByUserIdQuery(userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetNotificationPreferencesQuery_WithValidId_ShouldReturnPreferences()
    {
        // Arrange
        var userProfileId = Guid.NewGuid();
        var userProfile = new UserProfile(userProfileId, Guid.NewGuid(), "user@example.com");
        userProfile.UpdateNotificationPreferences(false, true, true, false, true);

        var mockRepository = new Mock<IUserProfileRepository>();
        mockRepository.Setup(r => r.GetByIdAsync(userProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userProfile);

        var mockMapper = new Mock<IMapper>();
        var expectedDto = new NotificationPreferencesDto
        {
            EmailNotifications = false,
            PushNotifications = true
        };
        mockMapper.Setup(m => m.Map<NotificationPreferencesDto>(It.IsAny<object>()))
            .Returns(expectedDto);

        var handler = new GetNotificationPreferencesQueryHandler(mockRepository.Object, mockMapper.Object);
        var query = new GetNotificationPreferencesQuery(userProfileId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.EmailNotifications.Should().BeFalse();
        result.PushNotifications.Should().BeTrue();
    }

    [Fact]
    public async Task GetNotificationPreferencesQuery_WithNonexistentId_ShouldThrowNotFoundException()
    {
        // Arrange
        var userProfileId = Guid.NewGuid();
        var mockRepository = new Mock<IUserProfileRepository>();
        mockRepository.Setup(r => r.GetByIdAsync(userProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfile?)null);

        var mockMapper = new Mock<IMapper>();
        var handler = new GetNotificationPreferencesQueryHandler(mockRepository.Object, mockMapper.Object);
        var query = new GetNotificationPreferencesQuery(userProfileId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task GetUserProfileByUserIdQuery_WithInactiveProfile_ShouldThrowNotFoundException()
    {
        // Arrange
        var userProfileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var userProfile = new UserProfile(userProfileId, userId, "user@example.com");
        userProfile.Deactivate();

        var mockRepository = new Mock<IUserProfileRepository>();
        mockRepository.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userProfile);

        var mockMapper = new Mock<IMapper>();
        var handler = new GetUserProfileByUserIdQueryHandler(mockRepository.Object, mockMapper.Object);
        var query = new GetUserProfileByUserIdQuery(userId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None));
    }
}
