using AutoMapper;
using Identity.Application.Contracts;
using Identity.Application.Features.Authentication;
using Identity.Domain.Constants;
using Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.Configuration;
using SharedKernel.Models;
using System.Data;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace Identity.Infrastructure.Services;

public partial class AuthenticationService(
    ILogger<AuthenticationService> logger,
    RequestCorrelationId requestCorrelationId,
    IMapper mapper,
    UserManager<ApplicationUser> userManager,
    JwtConfig jwtConfig) : IAuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger = logger;
    private readonly RequestCorrelationId _requestCorrelationId = requestCorrelationId;
    private readonly IMapper _mapper = mapper;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly JwtConfig _jwtConfig = jwtConfig;

    public async Task<LoginResult> Login(LoginDto loginModel, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(loginModel.Email);

        if (user is null || !await _userManager.CheckPasswordAsync(user, loginModel.Password))
        {
            _logger.LogInformation($"[{_requestCorrelationId.Id}] Invalid Credentials for {loginModel.Email}");
            return new LoginResult(null, AuthenticationMessageConstants.InvalidCredentials);
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        var authClaims = new List<Claim>
        {
            new(ClaimTypes.Sid, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.GivenName, user.UserName),
            new(ClaimTypes.Role, userRoles.First()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Secret));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtConfig.ValidIssuer,
            Audience = _jwtConfig.ValidAudience,
            Expires = DateTime.Now.AddHours(_jwtConfig.ValidHours),
            Subject = new ClaimsIdentity(authClaims),
            SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        };

        var tokenHandler = new JsonWebTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        _logger.LogInformation($"[{_requestCorrelationId.Id}] Login success for {loginModel.Email}");
        return new LoginResult(token, AuthenticationMessageConstants.SuccessfulLogin);
    }

    public async Task<RegistrationResult> Register(RegisterDto registerModel, CancellationToken cancellationToken)
    {
        if (!IsValidModel(registerModel))
            return new RegistrationResult(RegistrationStatus.Failed, AuthenticationMessageConstants.NullOrEmpty);

        if (!IsEmailValid(registerModel.Email))
            return new RegistrationResult(RegistrationStatus.Failed, AuthenticationMessageConstants.InvalidEmail);

        var emailInUse = await _userManager.FindByEmailAsync(registerModel.Email);
        if (emailInUse is not null)
        {
            _logger.LogInformation($"[{_requestCorrelationId.Id}] Email in use for {registerModel.Email}");
            return new RegistrationResult(RegistrationStatus.Failed, AuthenticationMessageConstants.EmailInUse);
        }

        var user = _mapper.Map<ApplicationUser>(registerModel);

        var creationResult = await _userManager.CreateAsync(user, registerModel.Password);
        var identityErrors = creationResult.Errors.Select(x => x.Description).ToList();
        if (identityErrors.Count != 0)
            return new RegistrationResult(RegistrationStatus.Failed, string.Join("\n", identityErrors));

        var roleResult = await _userManager.AddToRoleAsync(user, ApplicationRoles.User);
        var roleErrors = roleResult.Errors.Select(x => x.Description).ToList();
        if (roleErrors.Count != 0)
            return new RegistrationResult(RegistrationStatus.Failed, string.Join("\n", roleErrors));

        _logger.LogInformation($"[{_requestCorrelationId.Id}] Register success for Username: {registerModel.Username}, Email: {registerModel.Email}]");
        return new RegistrationResult(RegistrationStatus.Successful, AuthenticationMessageConstants.SuccessfulRegistration);
    }

    private static bool IsValidModel(RegisterDto registerModel)
    {
        return !string.IsNullOrEmpty(registerModel.Email)
            && !string.IsNullOrEmpty(registerModel.Username)
            && !string.IsNullOrEmpty(registerModel.Password)
            && !string.IsNullOrEmpty(registerModel.FirstName)
            && !string.IsNullOrEmpty(registerModel.LastName)
            && !string.IsNullOrEmpty(registerModel.PhoneNumber);
    }

    private static bool IsEmailValid(string email) => StrongPasswordRegex().Match(email).Success;

    [GeneratedRegex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$")]
    private static partial Regex StrongPasswordRegex();
}
