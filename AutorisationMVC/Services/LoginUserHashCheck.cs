namespace AutorisationMVC.Services;

public sealed class LoginUserHashCheck(IUserRepository userRepository,IPasswordHasher passwordHasher)
{
    public record Request (string Email, string Password);
}