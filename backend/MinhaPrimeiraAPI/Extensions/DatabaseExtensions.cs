using Microsoft.EntityFrameworkCore;
using MinhaPrimeiraAPI.Data;

namespace MinhaPrimeiraAPI.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddApplicationDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "A connection string 'DefaultConnection' não foi configurada."
            );

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(connectionString);
        });

        return services;
    }
}
