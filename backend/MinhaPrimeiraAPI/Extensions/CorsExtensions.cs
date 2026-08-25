namespace MinhaPrimeiraAPI.Extensions;

public static class CorsExtensions
{
    private const string FrontendPolicy = "FrontendReact";

    public static IServiceCollection AddApplicationCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var origensPermitidas = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()
            ?? new[] { "http://localhost:5173" };

        services.AddCors(options =>
        {
            options.AddPolicy(FrontendPolicy, policy =>
            {
                policy
                    .WithOrigins(origensPermitidas)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    public static WebApplication UseApplicationCors(
        this WebApplication app)
    {
        app.UseCors(FrontendPolicy);
        return app;
    }
}
