using AutoMapper;
using Identity.Application.Contracts;
using Identity.Application.Features.Authentication;
using Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Identity.Infrastructure.Services;

public class AuthenticationService(
    IMapper mapper,
    UserManager<ApplicationUser> userManager) : IAuthenticationService
{
    private readonly IMapper _mapper = mapper;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<RegistrationResult> Register(RegisterDto registerModel)
    {
        var user = _mapper.Map<ApplicationUser>(registerModel);
        var identityResult = await _userManager.CreateAsync(user, registerModel.Password);

        var errors = identityResult.Errors.Select(x => x.Description).ToList();

        return identityResult.Succeeded
            ? new RegistrationResult(RegistrationStatus.Successful, "Registration was successful!")
            : new RegistrationResult(RegistrationStatus.Failed, string.Join("\n", errors));
    }
}
