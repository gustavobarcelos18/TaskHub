using System.Security.Claims;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace ProjetoTarefas.Extensions;

public sealed class SqliteLogSink : ILogEventSink, IDisposable
{
    private static readonly string[] Allowlist = ["TarefaId", "Motivo", "Resultado", "Campo", "TraceId", "RequestId"];
    private readonly SqliteConnection _connection;
    private readonly object _gate = new();

    public SqliteLogSink(string connectionString, int retentionDays)
    {
        _connection = new SqliteConnection(connectionString);
        _connection.Open();
        using var command = _connection.CreateCommand();
        command.CommandText = """
            PRAGMA journal_mode=WAL;
            CREATE TABLE IF NOT EXISTS LogEventos (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                TimestampUtc TEXT NOT NULL,
                Level TEXT NOT NULL,
                EventName TEXT NOT NULL,
                Message TEXT NOT NULL,
                UserId TEXT NULL,
                UserName TEXT NULL,
                Method TEXT NULL,
                Path TEXT NULL,
                StatusCode INTEGER NULL,
                ElapsedMs REAL NULL,
                TraceId TEXT NULL,
                SafePropertiesJson TEXT NULL
            );
            CREATE INDEX IF NOT EXISTS IX_LogEventos_TimestampUtc ON LogEventos (TimestampUtc DESC, Id DESC);
            CREATE INDEX IF NOT EXISTS IX_LogEventos_Level ON LogEventos (Level);
            CREATE INDEX IF NOT EXISTS IX_LogEventos_TraceId ON LogEventos (TraceId);
            DELETE FROM LogEventos WHERE TimestampUtc < $cutoff;
            """;
        command.Parameters.AddWithValue("$cutoff", DateTime.UtcNow.AddDays(-Math.Max(retentionDays, 1)).ToString("O"));
        command.ExecuteNonQuery();
    }

    public void Emit(LogEvent logEvent)
    {
        try
        {
            var props = new Dictionary<string, string>();
            foreach (var name in Allowlist)
                if (logEvent.Properties.TryGetValue(name, out var value)) props[name] = value.ToString().Trim('"');

            var requestMethod = Valor(logEvent, "RequestMethod");
            var requestPath = Valor(logEvent, "RequestPath");
            var statusCode = Valor(logEvent, "StatusCode");
            var elapsed = Valor(logEvent, "Elapsed");
            var traceId = Valor(logEvent, "TraceId") ?? Valor(logEvent, "RequestId");
            var eventName = Valor(logEvent, "EventName") ?? (requestMethod is null ? logEvent.Properties.TryGetValue("SourceContext", out var source) ? source.ToString().Trim('"') : "Application" : "HttpRequest");
            var userId = Valor(logEvent, "UserId");
            var userName = Valor(logEvent, "UserName");
            lock (_gate)
            {
                using var command = _connection.CreateCommand();
                command.CommandText = "INSERT INTO LogEventos (TimestampUtc, Level, EventName, Message, UserId, UserName, Method, Path, StatusCode, ElapsedMs, TraceId, SafePropertiesJson) VALUES ($timestamp, $level, $event, $message, $userId, $userName, $method, $path, $status, $elapsed, $trace, $properties)";
                command.Parameters.AddWithValue("$timestamp", logEvent.Timestamp.UtcDateTime.ToString("O"));
                command.Parameters.AddWithValue("$level", logEvent.Level.ToString()); command.Parameters.AddWithValue("$event", eventName);
                command.Parameters.AddWithValue("$message", logEvent.RenderMessage()); Add(command, "$userId", userId); Add(command, "$userName", userName);
                Add(command, "$method", requestMethod); Add(command, "$path", requestPath); Add(command, "$status", int.TryParse(statusCode, out var status) ? status : null);
                Add(command, "$elapsed", double.TryParse(elapsed, out var duration) ? duration : null); Add(command, "$trace", traceId);
                Add(command, "$properties", props.Count == 0 ? null : JsonSerializer.Serialize(props)); command.ExecuteNonQuery();
            }
        }
        catch { /* Observabilidade não pode interromper a requisição principal. */ }
    }

    private static string? Valor(LogEvent logEvent, string name) => logEvent.Properties.TryGetValue(name, out var value) ? value.ToString().Trim('"') : null;
    private static void Add(SqliteCommand command, string name, object? value) => command.Parameters.AddWithValue(name, value ?? DBNull.Value);
    public void Dispose() => _connection.Dispose();
}

public static class TechnicalLoggingExtensions
{
    public static WebApplicationBuilder AddTechnicalLogStorage(this WebApplicationBuilder builder)
    {
        var options = builder.Configuration.GetSection("TechnicalLogging");
        var relativePath = options["ConnectionString"] ?? "Data Source=Database/logs.db";
        var connectionString = relativePath.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase)
            ? relativePath : $"Data Source={Path.Combine(builder.Environment.ContentRootPath, relativePath)}";
        var dataSource = new SqliteConnectionStringBuilder(connectionString).DataSource;
        if (!Path.IsPathFullyQualified(dataSource)) dataSource = Path.Combine(builder.Environment.ContentRootPath, dataSource);
        Directory.CreateDirectory(Path.GetDirectoryName(dataSource)!);
        var sink = new SqliteLogSink(new SqliteConnectionStringBuilder(connectionString) { DataSource = dataSource }.ToString(), options.GetValue("RetentionDays", 30));
        builder.Services.AddSingleton(sink);
        Log.Logger = new LoggerConfiguration().MinimumLevel.Information().MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning).MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning).Enrich.FromLogContext().Enrich.WithProperty("Aplicacao", "ProjetoTarefas").WriteTo.Console().WriteTo.File(Path.Combine(builder.Environment.ContentRootPath, "Logs", "api-.log"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30, rollOnFileSizeLimit: true, fileSizeLimitBytes: 10_000_000, shared: true, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj} | RequestId={RequestId}{NewLine}{Exception}").WriteTo.Sink(sink).CreateLogger();
        return builder;
    }

    public static WebApplication UseTechnicalLogContext(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var user = context.User;
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = user.FindFirstValue(ClaimTypes.Email) ?? user.Identity?.Name;
            using (Serilog.Context.LogContext.PushProperty("TraceId", context.TraceIdentifier))
            using (Serilog.Context.LogContext.PushProperty("UserId", userId ?? "anonymous"))
            using (Serilog.Context.LogContext.PushProperty("UserName", userName ?? "Anônimo"))
                await next(context);
        });
        return app;
    }
}
