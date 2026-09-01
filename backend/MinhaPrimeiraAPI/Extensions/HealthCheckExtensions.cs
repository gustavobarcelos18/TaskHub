using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ProjetoTarefas.Data;

namespace ProjetoTarefas.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddApplicationHealthChecks(
        this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<ApiHealthCheck>("api", tags: ["critical"])
            .AddCheck<SqliteHealthCheck>("sqlite", failureStatus: HealthStatus.Unhealthy, tags: ["critical"])
            .AddCheck<PersistenceHealthCheck>("persistence", failureStatus: HealthStatus.Unhealthy, tags: ["critical"])
            .AddCheck<FilesystemHealthCheck>("filesystem", failureStatus: HealthStatus.Unhealthy, tags: ["critical"]);

        return services;
    }
}

public sealed class ApiHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(HealthCheckResult.Healthy());
}

public sealed class SqliteHealthCheck(IServiceScopeFactory scopeFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await database.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy();
    }
}

public sealed class PersistenceHealthCheck(IServiceScopeFactory scopeFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await database.Tarefas.AsNoTracking().Select(_ => 1).Take(1).ToListAsync(cancellationToken);
        return HealthCheckResult.Healthy();
    }
}

public sealed class FilesystemHealthCheck(
    IConfiguration configuration,
    IHostEnvironment environment) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            return Task.FromResult(HealthCheckResult.Unhealthy());

        var dataSource = new SqliteConnectionStringBuilder(connectionString).DataSource;
        if (string.IsNullOrWhiteSpace(dataSource) || dataSource == ":memory:")
            return Task.FromResult(HealthCheckResult.Healthy());

        var databasePath = Path.IsPathFullyQualified(dataSource)
            ? dataSource
            : Path.Combine(environment.ContentRootPath, dataSource);
        var directory = Path.GetDirectoryName(databasePath);

        var healthy = !string.IsNullOrWhiteSpace(directory)
            && Directory.Exists(directory)
            && File.Exists(databasePath);
        return Task.FromResult(healthy
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy());
    }
}
