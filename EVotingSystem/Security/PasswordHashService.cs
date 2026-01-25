using System;
using System.Security.Cryptography;
using System.Text;

namespace EVotingSystem.Security
{
    public static class PasswordHashService
    {
        private const int Iterations = 100_000;
        private const int HashSize = 32;

        public static string HashPassword(string password, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256);

            return Convert.ToBase64String(pbkdf2.GetBytes(HashSize));
        }

        public static bool VerifyPassword(
            string enteredPassword,
            string storedHash,
            byte[] salt)
        {
            string computedHash = HashPassword(enteredPassword, salt);
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(computedHash),
                Convert.FromBase64String(storedHash));
        }

        public static byte[] GenerateSalt()
        {
            byte[] salt = new byte[16];
            RandomNumberGenerator.Fill(salt);
            return salt;
        }
    }
}
