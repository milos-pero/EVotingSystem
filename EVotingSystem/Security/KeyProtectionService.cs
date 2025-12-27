using System;
using System.Security.Cryptography;
using System.Text;

namespace EVotingSystem.Security
{
    internal static class KeyProtectionService
    {
        private const int SaltSize = 16;          // 128-bit salt
        private const int Iterations = 100_000;   // PBKDF2 iterations
        private const int KeySize = 32;            // 256-bit key

        /// <summary>
        /// Generates a cryptographically secure random salt.
        /// </summary>
        public static byte[] GenerateSalt()
        {
            return RandomNumberGenerator.GetBytes(SaltSize);
        }

        /// <summary>
        /// Derives a key from a password and salt using PBKDF2.
        /// </summary>
        public static byte[] DeriveKey(string password, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256);

            return pbkdf2.GetBytes(KeySize);
        }

        /// <summary>
        /// Produces a string suitable for use as a PFX password.
        /// </summary>
        public static string DerivePfxPassword(string password, byte[] salt)
        {
            byte[] key = DeriveKey(password, salt);
            return Convert.ToBase64String(key);
        }
    }
}
