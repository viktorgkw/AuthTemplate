namespace Identity.Domain.Constants;

public static class AuthenticationMessageConstants
{
    // Register
    public static readonly string SuccessfulRegistration = "Registration was successful!";
    public static readonly string InvalidEmail = "Email is not valid!";
    public static readonly string EmailInUse = "Email is already in use!";

    // Login
    public static readonly string SuccessfulLogin = "Login was successful!";
    public static readonly string InvalidCredentials = "Invalid credentials!";
}
