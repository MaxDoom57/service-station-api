using System.Security.Cryptography;
using System.Text;

namespace Shared.Utilities
{
    public static class PasswordHasher
    {
        public static string Hash(string data)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hashBytes);
        }

        public static bool Verify(string input, string hashed)
        {
            return Hash(input) == hashed;
        }
    }
}
