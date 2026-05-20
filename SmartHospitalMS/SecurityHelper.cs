using System;
using System.Security.Cryptography;
using System.Text;

namespace SmartHospitalMS
{
    /// <summary>
    /// SecurityHelper handles password hashing.
    /// JS Analogy: Like using 'bcrypt' in Node.js.
    /// </summary>
    public static class SecurityHelper
    {
        /// <summary>
        /// Hashes a string using SHA-256.
        /// </summary>
        public static string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convert byte array to a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Verifies a plain text password against a stored hash.
        /// </summary>
        public static bool VerifyPassword(string inputPassword, string storedHash)
        {
            string hashOfInput = HashPassword(inputPassword);
            return string.Equals(hashOfInput, storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
