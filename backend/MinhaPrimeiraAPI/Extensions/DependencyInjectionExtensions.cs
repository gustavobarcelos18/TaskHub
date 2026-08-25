using MinhaPrimeiraAPI.Repositories;
using MinhaPrimeiraAPI.Services;

namespace MinhaPrimeiraAPI.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddControllers();

        services.AddScoped<ITarefaRepository, TarefaRepository>();
        services.AddScoped<ITarefaService, TarefaService>();

        return services;
    }
}
