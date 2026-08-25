using Serilog;
using Serilog.Events;

namespace MinhaPrimeiraAPI.Extensions;

public static class LoggingExtensions
{
    public static WebApplicationBuilder AddApplicationLogging(
        this WebApplicationBuilder builder)
    {
        var pastaLogs = Path.Combine(
            builder.Environment.ContentRootPath,
            "Logs"
        );

        Directory.CreateDirectory(pastaLogs);

        var caminhoLog = Path.Combine(
            pastaLogs,
            "api-.log"
        );

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override(
                "Microsoft.AspNetCore",
                LogEventLevel.Warning
            )
            .Enrich.FromLogContext()
            .Enrich.WithProperty(
                "Aplicacao",
                "MinhaPrimeiraAPI"
            )
            .WriteTo.Console()
            .WriteTo.File(
                path: caminhoLog,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                rollOnFileSizeLimit: true,
                fileSizeLimitBytes: 10_000_000,
                shared: true,
                outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} " +
                    "[{Level:u3}] " +
                    "[{SourceContext}] " +
                    "{Message:lj} " +
                    "| RequestId={RequestId}" +
                    "{NewLine}{Exception}"
            )
            .CreateLogger();

        builder.Services.AddSerilog();

        return builder;
    }

    public static WebApplication UseApplicationRequestLogging(
        this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        return app;
    }
}
