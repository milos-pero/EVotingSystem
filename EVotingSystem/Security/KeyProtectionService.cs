using System;
using System.Security.Cryptography;
using System.Text;

namespace EVotingSystem.Security
{
    internal static class KeyProtectionService
    {
        private const int SaltSize = 16;
        private const int Iterations = 100_000;
        private const int KeySize = 32;

        // Generate a random salt.
        public static byte[] GenerateSalt()
        {
            return RandomNumberGenerator.GetBytes(SaltSize);
        }

        // Derive a key from password and salt using PBKDF2.
        public static byte[] DeriveKey(string password, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256);

            return pbkdf2.GetBytes(KeySize);
        }

        // Generate PFX password
        public static string DerivePfxPassword(string password, byte[] salt)
        {
            byte[] key = DeriveKey(password, salt);
            return Convert.ToBase64String(key);
        }
    }
}
