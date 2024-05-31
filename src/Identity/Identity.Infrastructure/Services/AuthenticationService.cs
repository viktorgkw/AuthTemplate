using AutoMapper;
using Identity.Application.Contracts;
using Identity.Application.Features.Authentication;
using Identity.Domain.Configuration;
using Identity.Domain.Constants;
using Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace Identity.Infrastructure.Services;

public partial class AuthenticationService(
    IMapper mapper,
    UserManager<ApplicationUser> userManager,
    JwtConfig jwtConfig) : IAuthenticationService
{
    private readonly IMapper _mapper = mapper;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly JwtConfig _jwtConfig = jwtConfig;

    public async Task<LoginResult> Login(LoginDto loginModel, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(loginModel.Email);

        if (user is null || !await _userManager.CheckPasswordAsync(user, loginModel.Password))
            return new LoginResult(null, AuthenticationMessageConstants.InvalidCredentials);

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

        return new LoginResult(token, AuthenticationMessageConstants.SuccessfulLogin);
    }

    public async Task<RegistrationResult> Register(RegisterDto registerModel, CancellationToken cancellationToken)
    {
        if (!IsEmailValid(registerModel.Email))
            return new RegistrationResult(RegistrationStatus.Failed, AuthenticationMessageConstants.InvalidEmail);

        var emailInUse = await _userManager.FindByEmailAsync(registerModel.Email);
        if (emailInUse is not null)
            return new RegistrationResult(RegistrationStatus.Failed, AuthenticationMessageConstants.EmailInUse);

        var user = _mapper.Map<ApplicationUser>(registerModel);
        var identityResult = await _userManager.CreateAsync(user, registerModel.Password);

        var errors = identityResult.Errors.Select(x => x.Description).ToList();

        if (errors.Count != 0)
            return new RegistrationResult(RegistrationStatus.Failed, string.Join("\n", errors));

        await _userManager.AddToRoleAsync(user, ApplicationRoles.User);

        return new RegistrationResult(RegistrationStatus.Successful, AuthenticationMessageConstants.SuccessfulRegistration);
    }

    private static bool IsEmailValid(string email) => StrongPasswordRegex().Match(email).Success;

    [GeneratedRegex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$")]
    private static partial Regex StrongPasswordRegex();
}
