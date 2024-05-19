namespace Identity.Domain.Constants;

public static class ApplicationRoles
{
    public static readonly string[] Roles = [
        nameof(User),
        nameof(Administrator),
    ];

    public static readonly string User = "User";
    public static readonly string Administrator = "Administrator";
}
