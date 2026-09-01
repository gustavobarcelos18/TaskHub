using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ProjetoTarefas.DTOs.Responses;

namespace ProjetoTarefas.Services;

public sealed class HealthDiagnosticsService(
    HealthCheckService healthCheckService,
    TimeProvider timeProvider)
{
    private readonly long _startedAtTimestamp = Stopwatch.GetTimestamp();

    public async Task<HealthDetalhadoResponse> ObterDetalhesAsync(CancellationToken cancellationToken)
    {
        var checkedAt = timeProvider.GetUtcNow();
        var report = await healthCheckService.CheckHealthAsync(cancellationToken);

        return new HealthDetalhadoResponse
        {
            Status = report.Status.ToString(),
            UptimeSeconds = (long)Stopwatch.GetElapsedTime(_startedAtTimestamp).TotalSeconds,
            CheckedAt = checkedAt,
            Checks = report.Entries.Select(entry => new HealthCheckDetalhadoResponse
            {
                Name = entry.Key,
                Status = entry.Value.Status.ToString(),
                DurationMs = Math.Round(entry.Value.Duration.TotalMilliseconds, 2),
            }).ToArray(),
        };
    }
}
