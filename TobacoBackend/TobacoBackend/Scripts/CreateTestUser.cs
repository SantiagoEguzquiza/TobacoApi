using System.Security.Cryptography;
using System.Text;

namespace TobacoBackend.Scripts
{
    public static class CreateTestUser
    {
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public static void Main()
        {
            // Test user credentials
            var userName = "admin";
            var password = "admin123";
            var hashedPassword = HashPassword(password);

            Console.WriteLine("=== Test User Creation ===");
            Console.WriteLine($"Username: {userName}");
            Console.WriteLine($"Password: {password}");
            Console.WriteLine($"Hashed Password: {hashedPassword}");
            Console.WriteLine();
            Console.WriteLine("SQL to insert test user:");
            Console.WriteLine($"INSERT INTO Users (UserName, Password, Email, CreatedAt, IsActive) VALUES ('{userName}', '{hashedPassword}', 'admin@tobaco.com', GETUTCDATE(), 1);");
        }
    }
}
