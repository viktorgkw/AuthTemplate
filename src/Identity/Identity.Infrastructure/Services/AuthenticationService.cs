using AutoMapper;
using Identity.Application.Contracts;
using Identity.Application.Features.Authentication;
using Identity.Domain.Constants;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Text.RegularExpressions;

namespace Identity.Infrastructure.Services;

public partial class AuthenticationService(
    IMapper mapper,
    UserManager<ApplicationUser> userManager) : IAuthenticationService
{
    private readonly IMapper _mapper = mapper;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<RegistrationResult> Register(RegisterDto registerModel, CancellationToken cancellationToken)
    {
        if (!IsEmailValid(registerModel.Email))
            return new RegistrationResult(RegistrationStatus.Failed, RegistrationMessageConstants.InvalidEmail);

        var emailInUse = await _userManager.FindByEmailAsync(registerModel.Email);
        if (emailInUse is not null)
            return new RegistrationResult(RegistrationStatus.Failed, RegistrationMessageConstants.EmailInUse);

        var user = _mapper.Map<ApplicationUser>(registerModel);
        var identityResult = await _userManager.CreateAsync(user, registerModel.Password);

        var errors = identityResult.Errors.Select(x => x.Description).ToList();

        if (errors.Count != 0)
            return new RegistrationResult(RegistrationStatus.Failed, string.Join("\n", errors));

        await _userManager.AddToRoleAsync(user, ApplicationRoles.User.ToString());

        return new RegistrationResult(RegistrationStatus.Successful, RegistrationMessageConstants.SuccessfulRegistration);
    }

    private static bool IsEmailValid(string email) => StrongPasswordRegex().Match(email).Success;

    [GeneratedRegex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$")]
    private static partial Regex StrongPasswordRegex();
}
