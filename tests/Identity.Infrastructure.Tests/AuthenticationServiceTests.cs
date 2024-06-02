using AutoMapper;
using Identity.Application.Features.Authentication;
using Identity.Domain.Constants;
using Identity.Domain.Entities;
using Identity.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using SharedKernel.Configuration;
using SharedKernel.Models;

namespace Identity.Infrastructure.Tests;

public class AuthenticationServiceTests
{
    #region Fields
    private readonly Mock<ILogger<AuthenticationService>> _loggerMock;
    private readonly Mock<RequestCorrelationId> _requestCorrelationIdMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly JwtConfig _jwtConfig;
    private readonly AuthenticationService _authenticationService;
    #endregion

    #region Constructor
    public AuthenticationServiceTests()
    {
        _loggerMock = new Mock<ILogger<AuthenticationService>>();
        _requestCorrelationIdMock = new Mock<RequestCorrelationId>();
        _mapperMock = new Mock<IMapper>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

        _jwtConfig = new JwtConfig
        {
            Secret = "97ynv35ty4t8943f593tbg437tf3iro27tr2or729t622yrd9n2t7rb97t234f352",
            ValidIssuer = "issuer",
            ValidAudience = "audience",
            ValidHours = 1
        };

        _authenticationService = new AuthenticationService(
            _loggerMock.Object,
            _requestCorrelationIdMock.Object,
            _mapperMock.Object,
            _userManagerMock.Object,
            _jwtConfig
        );
    }
    #endregion

    #region Login Tests
    [Fact]
    public async Task Login_ShouldReturnInvalidCredentials_WhenUserIsNotFound()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "notFound@example.com", Password = "17b4897#$b8rfvt31@4s8d5vd3" };
        _userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _authenticationService.Login(loginDto, CancellationToken.None);

        // Assert
        Assert.Null(result.Token);
        Assert.Equal(AuthenticationMessageConstants.InvalidCredentials, result.Message);
    }

    [Fact]
    public async Task Login_ShouldReturnInvalidCredentials_WhenPasswordIsIncorrect()
    {
        // Arrange
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "test@example.com", UserName = "testuser" };
        var loginDto = new LoginDto { Email = "test@example.com", Password = "wrongpassword" };

        _userManagerMock.Setup(um => um
            .FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(um => um
            .CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await _authenticationService.Login(loginDto, CancellationToken.None);

        // Assert
        Assert.Null(result.Token);
        Assert.Equal(AuthenticationMessageConstants.InvalidCredentials, result.Message);
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "test@example.com", UserName = "testuser" };
        var loginDto = new LoginDto { Email = "test@example.com", Password = "correctpassword" };
        var roles = new List<string> { "User" };

        _userManagerMock.Setup(um => um
            .FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        _userManagerMock.Setup(um => um
            .CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _userManagerMock.Setup(um => um
            .GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(roles);

        // Act
        var result = await _authenticationService.Login(loginDto, CancellationToken.None);

        // Assert
        Assert.NotNull(result.Token);
        Assert.Equal(AuthenticationMessageConstants.SuccessfulLogin, result.Message);
    }
    #endregion

    #region Register Tests
    [Fact]
    public async Task Register_ShouldReturnInvalidEmail_WhenEmailIsInvalid()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "invalidemail",
            Password = "password",
            Username = "testuser",
            FirstName = "test",
            LastName = "test",
            PhoneNumber = "test",
        };

        // Act
        var result = await _authenticationService.Register(registerDto, CancellationToken.None);

        // Assert
        Assert.Equal(RegistrationStatus.Failed, result.Status);
        Assert.Equal(AuthenticationMessageConstants.InvalidEmail, result.Message);
    }

    [Fact]
    public async Task Register_ShouldReturnEmailInUse_WhenEmailAlreadyExists()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password",
            Username = "testuser",
            FirstName = "test",
            LastName = "test",
            PhoneNumber = "test",
        };

        var existingUser = new ApplicationUser { Id = Guid.NewGuid(), Email = "test@example.com" };
        _userManagerMock.Setup(um => um
            .FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _authenticationService.Register(registerDto, CancellationToken.None);

        // Assert
        Assert.Equal(RegistrationStatus.Failed, result.Status);
        Assert.Equal(AuthenticationMessageConstants.EmailInUse, result.Message);
    }

    [Fact]
    public async Task Register_ShouldReturnErrors_WhenUserCreationFails()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password",
            Username = "testuser",
            FirstName = "test",
            LastName = "test",
            PhoneNumber = "test",
        };

        var identityResult = IdentityResult.Failed(
            new IdentityError { Description = "Error1" },
            new IdentityError { Description = "Error2" });

        _userManagerMock.Setup(um => um
            .FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser)null);

        _mapperMock.Setup(m => m
            .Map<ApplicationUser>(It.IsAny<RegisterDto>()))
            .Returns(new ApplicationUser { Email = registerDto.Email, UserName = registerDto.Username });

        _userManagerMock.Setup(um => um
            .CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _authenticationService.Register(registerDto, CancellationToken.None);

        // Assert
        Assert.Equal(RegistrationStatus.Failed, result.Status);
        Assert.Equal("Error1\nError2", result.Message);
    }

    [Fact]
    public async Task Register_ShouldReturnSuccessfulRegistration_WhenUserIsCreatedSuccessfully()
    {
        // Arrange
        var identityResult = IdentityResult.Success;
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password",
            Username = "testuser",
            FirstName = "test",
            LastName = "test",
            PhoneNumber = "test",
        };

        _userManagerMock.Setup(um => um
            .FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser)null);

        _mapperMock.Setup(m => m
            .Map<ApplicationUser>(It.IsAny<RegisterDto>()))
            .Returns(new ApplicationUser { Email = registerDto.Email, UserName = registerDto.Username });

        _userManagerMock.Setup(um => um
            .CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(identityResult);

        _userManagerMock.Setup(um => um
            .AddToRoleAsync(It.IsAny<ApplicationUser>(), ApplicationRoles.User))
            .Returns(Task.FromResult(identityResult));

        // Act
        var result = await _authenticationService.Register(registerDto, CancellationToken.None);

        // Assert
        Assert.Equal(RegistrationStatus.Successful, result.Status);
        Assert.Equal(AuthenticationMessageConstants.SuccessfulRegistration, result.Message);
    }

    [Fact]
    public async Task Register_ShouldReturnFailed_WhenAddingRoleFails()
    {
        // Arrange
        var identityResult = IdentityResult.Success;
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password",
            Username = "testuser",
            FirstName = "test",
            LastName = "test",
            PhoneNumber = "test",
        };

        _userManagerMock.Setup(um => um
            .FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser)null);

        _mapperMock.Setup(m => m
            .Map<ApplicationUser>(It.IsAny<RegisterDto>()))
            .Returns(new ApplicationUser { Email = registerDto.Email, UserName = registerDto.Username });

        _userManagerMock.Setup(um => um
            .CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(identityResult);

        _userManagerMock.Setup(um => um
            .AddToRoleAsync(It.IsAny<ApplicationUser>(), ApplicationRoles.User))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role addition failed" }));

        // Act
        var result = await _authenticationService.Register(registerDto, CancellationToken.None);

        // Assert
        Assert.Equal(RegistrationStatus.Failed, result.Status);
        Assert.Contains("Role addition failed", result.Message);
    }

    [Fact]
    public async Task Register_ShouldReturnInvalidEmail_WhenEmailIsNull()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = null,
            Password = "password",
            Username = "testuser",
            FirstName = "test",
            LastName = "test",
            PhoneNumber = "test",
        };

        // Act
        var result = await _authenticationService.Register(registerDto, CancellationToken.None);

        // Assert
        Assert.Equal(RegistrationStatus.Failed, result.Status);
        Assert.Equal(AuthenticationMessageConstants.NullOrEmpty, result.Message);
    }

    [Fact]
    public async Task Register_ShouldReturnInvalidEmail_WhenEmailIsEmpty()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "",
            Password = "password",
            Username = "testuser",
            FirstName = "test",
            LastName = "test",
            PhoneNumber = "test"
        };

        // Act
        var result = await _authenticationService.Register(registerDto, CancellationToken.None);

        // Assert
        Assert.Equal(RegistrationStatus.Failed, result.Status);
        Assert.Equal(AuthenticationMessageConstants.NullOrEmpty, result.Message);
    }

    [Fact]
    public async Task Register_ShouldReturnFailed_WhenPasswordIsNull()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = null,
            Username = "testuser",
            FirstName = "test",
            LastName = "test",
            PhoneNumber = "test",
        };

        // Act
        var result = await _authenticationService.Register(registerDto, CancellationToken.None);

        // Assert
        Assert.Equal(RegistrationStatus.Failed, result.Status);
        Assert.Contains(AuthenticationMessageConstants.NullOrEmpty, result.Message);
    }

    [Fact]
    public async Task Register_ShouldReturnFailed_WhenPasswordIsEmpty()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "",
            Username = "testuser",
            FirstName = "test",
            LastName = "test",
            PhoneNumber = "test",
        };

        // Act
        var result = await _authenticationService.Register(registerDto, CancellationToken.None);

        // Assert
        Assert.Equal(RegistrationStatus.Failed, result.Status);
        Assert.Contains(AuthenticationMessageConstants.NullOrEmpty, result.Message);
    }

    [Fact]
    public async Task Register_ShouldReturnFailed_WhenUsernameIsNull()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password",
            Username = null,
            FirstName = "test",
            LastName = "test",
            PhoneNumber = "test",
        };

        // Act
        var result = await _authenticationService.Register(registerDto, CancellationToken.None);

        // Assert
        Assert.Equal(RegistrationStatus.Failed, result.Status);
        Assert.Contains(AuthenticationMessageConstants.NullOrEmpty, result.Message);
    }

    [Fact]
    public async Task Register_ShouldReturnFailed_WhenUsernameIsEmpty()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password",
            Username = "",
            FirstName = "test",
            LastName = "test",
            PhoneNumber = "test",
        };

        // Act
        var result = await _authenticationService.Register(registerDto, CancellationToken.None);

        // Assert
        Assert.Equal(RegistrationStatus.Failed, result.Status);
        Assert.Contains(AuthenticationMessageConstants.NullOrEmpty, result.Message);
    }

    [Fact]
    public async Task Register_ShouldReturnFailed_WhenFirstNameIsNull()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password",
            Username = "testuser",
            FirstName = null,
            LastName = "test",
            PhoneNumber = "test",
        };

        // Act
        var result = await _authenticationService.Register(registerDto, CancellationToken.None);

        // Assert
        Assert.Equal(RegistrationStatus.Failed, result.Status);
        Assert.Contains(AuthenticationMessageConstants.NullOrEmpty, result.Message);
    }

    [Fact]
    public async Task Register_ShouldReturnFailed_WhenFirstNameIsEmpty()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password",
            Username = "testuser",
            FirstName = "",
            LastName = "test",
            PhoneNumber = "test",
        };

        // Act
        var result = await _authenticationService.Register(registerDto, CancellationToken.None);

        // Assert
        Assert.Equal(RegistrationStatus.Failed, result.Status);
        Assert.Contains(AuthenticationMessageConstants.NullOrEmpty, result.Message);
    }

    [Fact]
    public async Task Register_ShouldReturnFailed_WhenLastNameIsNull()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password",
            Username = "testuser",
            FirstName = "test",
            LastName = null,
            PhoneNumber = "test",
        };

        // Act
        var result = await _authenticationService.Register(registerDto, CancellationToken.None);

        // Assert
        Assert.Equal(RegistrationStatus.Failed, result.Status);
        Assert.Contains(AuthenticationMessageConstants.NullOrEmpty, result.Message);
    }

    [Fact]
    public async Task Register_ShouldReturnFailed_WhenLastNameIsEmpty()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password",
            Username = "testuser",
            FirstName = "test",
            LastName = "",
            PhoneNumber = "test",
        };

        // Act
        var result = await _authenticationService.Register(registerDto, CancellationToken.None);

        // Assert
        Assert.Equal(RegistrationStatus.Failed, result.Status);
        Assert.Contains(AuthenticationMessageConstants.NullOrEmpty, result.Message);
    }

    [Fact]
    public async Task Register_ShouldReturnFailed_WhenPhoneNumberIsNull()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password",
            Username = "testuser",
            FirstName = "test",
            LastName = "test",
            PhoneNumber = null,
        };

        // Act
        var result = await _authenticationService.Register(registerDto, CancellationToken.None);

        // Assert
        Assert.Equal(RegistrationStatus.Failed, result.Status);
        Assert.Contains(AuthenticationMessageConstants.NullOrEmpty, result.Message);
    }

    [Fact]
    public async Task Register_ShouldReturnFailed_WhenPhoneNumberIsEmpty()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password",
            Username = "testuser",
            FirstName = "test",
            LastName = "test",
            PhoneNumber = "",
        };

        // Act
        var result = await _authenticationService.Register(registerDto, CancellationToken.None);

        // Assert
        Assert.Equal(RegistrationStatus.Failed, result.Status);
        Assert.Contains(AuthenticationMessageConstants.NullOrEmpty, result.Message);
    }
    #endregion
}