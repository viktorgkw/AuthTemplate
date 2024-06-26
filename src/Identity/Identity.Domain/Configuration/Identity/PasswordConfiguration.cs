﻿namespace Identity.Domain.Configuration.Identity;

public class PasswordConfiguration
{
    public bool RequireDigit { get; set; }
    public bool RequireLowercase { get; set; }
    public bool RequireUppercase { get; set; }
    public bool RequireNonAlphanumeric { get; set; }
    public int RequiredLength { get; set; }
    public int RequiredUniqueChars { get; set; }
}
