namespace Identity.Domain.Configuration.Identity;

public class IdentityConfiguration
{
    public PasswordConfiguration Password { get; set; }

    public LockoutConfiguration Lockout { get; set; }

    public UserConfiguration User { get; set; }

    public SignInConfiguration SignIn { get; set; }
}
