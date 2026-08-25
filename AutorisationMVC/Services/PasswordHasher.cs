using System.Security.Cryptography;

namespace AutorisationMVC.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100000;

    private readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

    public string? HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);
        return $"{Convert.ToHexString(hash)}{Convert.ToHexString(salt)}";
    }
}