using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using MinhaPrimeiraAPI.Data;

namespace MinhaPrimeiraAPI.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddApplicationDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "A connection string 'DefaultConnection' não foi configurada."
            );
        }

        var sqliteConnectionStringBuilder = new SqliteConnectionStringBuilder(connectionString);

        if (string.IsNullOrWhiteSpace(sqliteConnectionStringBuilder.DataSource))
        {
            throw new InvalidOperationException(
                "A connection string 'DefaultConnection' precisa informar Data Source."
            );
        }

        if (sqliteConnectionStringBuilder.DataSource != ":memory:")
        {
            var databasePath = Path.IsPathFullyQualified(sqliteConnectionStringBuilder.DataSource)
                ? sqliteConnectionStringBuilder.DataSource
                : Path.Combine(environment.ContentRootPath, sqliteConnectionStringBuilder.DataSource);
            var databaseDirectory = Path.GetDirectoryName(databasePath);

            if (!string.IsNullOrWhiteSpace(databaseDirectory))
            {
                Directory.CreateDirectory(databaseDirectory);
            }
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(connectionString);
        });

        return services;
    }
}
