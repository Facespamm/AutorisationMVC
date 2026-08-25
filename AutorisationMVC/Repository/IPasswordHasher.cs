namespace AutorisationMVC;

public interface IPasswordHasher
{
    public string? HashPassword(string password);
}