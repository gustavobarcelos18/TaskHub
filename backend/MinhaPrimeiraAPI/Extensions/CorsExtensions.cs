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
            ?.Where(origem => !string.IsNullOrWhiteSpace(origem))
            .Select(origem => origem.Trim())
            .ToArray();

        if (origensPermitidas is not { Length: > 0 })
        {
            throw new InvalidOperationException(
                "A configuração 'Cors:AllowedOrigins' precisa conter ao menos uma origem."
            );
        }

        if (origensPermitidas.Any(origem =>
            !Uri.TryCreate(origem, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)))
        {
            throw new InvalidOperationException(
                "Cada origem em 'Cors:AllowedOrigins' precisa ser uma URL HTTP ou HTTPS absoluta."
            );
        }

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
