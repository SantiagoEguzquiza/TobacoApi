using Microsoft.EntityFrameworkCore;
using TobacoBackend.Domain.Models;
using TobacoBackend.Services;

namespace TobacoBackend.Scripts
{
    public class CreateTestUsers
    {
        public static async Task CreateTestUsersAsync(AplicationDbContext context)
        {
            // Check if users already exist
            var existingUsers = await context.Users.ToListAsync();
            if (existingUsers.Any())
            {
                Console.WriteLine("Users already exist. Skipping creation.");
                return;
            }

            // Create admin user
            var adminUser = new User
            {
                UserName = "admin",
                Password = UserService.HashPasswordForStorage("admin123"),
                Email = "admin@tobaco.com",
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Create employee user
            var employeeUser = new User
            {
                UserName = "employee",
                Password = UserService.HashPasswordForStorage("employee123"),
                Email = "employee@tobaco.com",
                Role = "Employee",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(adminUser);
            context.Users.Add(employeeUser);
            await context.SaveChangesAsync();

            Console.WriteLine("Test users created successfully!");
            Console.WriteLine("Admin user: admin / admin123");
            Console.WriteLine("Employee user: employee / employee123");
        }
    }
}
