using Identity.Domain.Enums;

namespace Identity.Domain.Models;

public record RegistrationResult(RegistrationStatus Status, string Message);