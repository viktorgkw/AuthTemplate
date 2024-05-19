namespace Identity.Domain.Configuration.Identity;

public class LockoutConfiguration
{
    public int DefaultLockoutTimeSpanMinutes { get; set; }
    public int MaxFailedAccessAttempts { get; set; }
    public bool AllowedForNewUsers { get; set; }
}
