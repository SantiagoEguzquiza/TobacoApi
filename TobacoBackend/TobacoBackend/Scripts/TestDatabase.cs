using Microsoft.EntityFrameworkCore;
using TobacoBackend.Domain.Models;

namespace TobacoBackend.Scripts
{
    public class TestDatabase
    {
        public static async Task TestDatabaseConnection(AplicationDbContext context)
        {
            try
            {
                Console.WriteLine("Testing database connection...");
                
                // Test if we can connect to the database
                var canConnect = await context.Database.CanConnectAsync();
                Console.WriteLine($"Can connect to database: {canConnect}");
                
                if (canConnect)
                {
                    // Test if Users table exists and has the Role column
                    var users = await context.Users.Take(1).ToListAsync();
                    Console.WriteLine($"Users table accessible: {users != null}");
                    
                    if (users.Any())
                    {
                        var firstUser = users.First();
                        Console.WriteLine($"First user: {firstUser.UserName}, Role: {firstUser.Role}");
                    }
                    else
                    {
                        Console.WriteLine("No users found in database");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database test error: {ex.Message}");
            }
        }
    }
}
