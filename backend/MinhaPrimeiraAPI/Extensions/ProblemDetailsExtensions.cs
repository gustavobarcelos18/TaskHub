using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MinhaPrimeiraAPI.Extensions;

public static class ProblemDetailsExtensions
{
    public static IServiceCollection AddApplicationProblemDetails(
        this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions["traceId"] =
                    context.HttpContext.TraceIdentifier;
            };
        });

        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }

    public static WebApplication UseApplicationExceptionHandler(
        this WebApplication app)
    {
        app.UseExceptionHandler();
        return app;
    }
}

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            return true;
        }

        logger.LogError(
            exception,
            "Exceção não tratada durante a requisição. TraceId={TraceId}",
            httpContext.TraceIdentifier
        );

        return await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Erro interno do servidor",
                    Detail = "Ocorreu um erro interno ao processar a requisição.",
                    Instance = httpContext.Request.Path
                }
            }
        );
    }
}
