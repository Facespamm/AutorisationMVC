namespace AutorisationMVC;

public interface IPasswordHasher
{
    bool Verify(string password, string passwordHash);
}