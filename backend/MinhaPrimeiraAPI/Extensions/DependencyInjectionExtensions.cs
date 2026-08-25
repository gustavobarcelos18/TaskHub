using MinhaPrimeiraAPI.Repositories;
using MinhaPrimeiraAPI.Services;

namespace MinhaPrimeiraAPI.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddControllers();
        services.AddApplicationProblemDetails();

        services.AddSingleton(TimeProvider.System);

        services.AddScoped<ITarefaRepository, TarefaRepository>();
        services.AddScoped<ITarefaService, TarefaService>();
        services.AddScoped<IEtiquetaRepository, EtiquetaRepository>();
        services.AddScoped<IEtiquetaService, EtiquetaService>();
        services.AddScoped<IProjetoRepository, ProjetoRepository>();
        services.AddScoped<IProjetoService, ProjetoService>();

        return services;
    }
}
