using System.Security.Cryptography;

namespace AutorisationMVC.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100000;
    private readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;
    public string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);
        return $"{Convert.ToHexString(hash)}{Convert.ToHexString(salt)}";
    }
    public bool Verify(string password, string passwordHash)
    {
        int hashHexLength = HashSize * 2;
        int saltHexLength = SaltSize * 2;

        if (passwordHash.Length != hashHexLength + saltHexLength)
        {
            return false;
        }
        string hashHex = passwordHash.Substring(0, hashHexLength);
        string saltHex = passwordHash.Substring(hashHexLength, saltHexLength);
        byte[] hash = Convert.FromHexString(hashHex);
        byte[] salt = Convert.FromHexString(saltHex);
        byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);
        return CryptographicOperations.FixedTimeEquals(hash, inputHash);
    }
}