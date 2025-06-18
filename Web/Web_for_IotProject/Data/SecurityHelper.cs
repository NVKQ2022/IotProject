
using System.Security.Cryptography;
using System.Text;

namespace Web_for_IotProject.Data
{

    public class SecurityHelper
    {
        private const string myPepper = "MyWebAppProject";

        public static string GenerateSessionId(int length = 32)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var data = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(data);
            }

            var result = new StringBuilder(length);
            foreach (byte b in data)
            {
                result.Append(chars[b % chars.Length]);
            }

            return result.ToString();
        }

        public static string GenerateSalt(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var saltBytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }

            var saltChars = new char[length];
            for (int i = 0; i < length; i++)
            {
                saltChars[i] = chars[saltBytes[i] % chars.Length];
            }

            return new string(saltChars);
        }

        public static string HashPassword(string password, string salt)
        {
            string combined = password + salt + myPepper;
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(combined));
                return Convert.ToBase64String(hashBytes);
            }
        }

        public static bool VerifyPassword(string inputPassword, string storedSalt, string storedHash)
        {
            string newHash = HashPassword(inputPassword, storedSalt);
            return storedHash == newHash;
        }
    }
}
