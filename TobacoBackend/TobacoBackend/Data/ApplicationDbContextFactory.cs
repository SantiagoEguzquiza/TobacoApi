using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using System.IO;
using System.Reflection;

namespace TobacoBackend.Data
{
    public class ApplicationDbContextFactory 
        : IDesignTimeDbContextFactory<AplicationDbContext>
    {
        public AplicationDbContext CreateDbContext(string[] args)
        {
            var basePath = FindAppsettingsBasePath();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    "DefaultConnection está vacía o no existe. Configure la cadena de conexión en appsettings.json o appsettings.Development.json. Ejemplo: \"Host=localhost;Port=5432;Database=railway;Username=postgres;Password=tu_password\"");

            var optionsBuilder = new DbContextOptionsBuilder<AplicationDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new AplicationDbContext(optionsBuilder.Options);
        }

        private static string FindAppsettingsBasePath()
        {
            var dir = Directory.GetCurrentDirectory();
            foreach (var path in new[]
            {
                dir,
                Path.Combine(dir, "TobacoBackend"),
                Path.Combine(dir, "TobacoBackend", "TobacoBackend"),
                Path.GetDirectoryName(Assembly.GetExecutingAssembly()?.Location) ?? dir
            })
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(Path.Combine(path, "appsettings.json")))
                    return path;
            }
            return dir;
        }
    }
}
