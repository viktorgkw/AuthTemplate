using Microsoft.AspNetCore.Identity;

namespace Identity.Domain.Entities;

/// <summary>
/// Inherited properties by IdentityUser:
/// Id, UserName, Email, EmailConfirmed, PasswordHash, PhoneNumber, PhoneNumberConfirmed
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public bool IsActive { get; set; }

    public Address Address { get; set; }
}
