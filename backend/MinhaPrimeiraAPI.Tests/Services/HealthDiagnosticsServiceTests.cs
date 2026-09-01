using Microsoft.Extensions.Diagnostics.HealthChecks;
using ProjetoTarefas.Services;

namespace ProjetoTarefas.Tests.Services;

public sealed class HealthDiagnosticsServiceTests
{
    [Fact]
    public async Task ObterDetalhes_deve_mapear_status_duracao_e_horario_sem_dados_internos()
    {
        var healthService = new HealthCheckServiceFake(new Dictionary<string, HealthReportEntry>
        {
            ["api"] = new(HealthStatus.Healthy, "não deve ser exposto", TimeSpan.FromMilliseconds(2), null, null),
            ["sqlite"] = new(HealthStatus.Degraded, null, TimeSpan.FromMilliseconds(8), null, null),
        });
        var agora = new DateTimeOffset(2026, 8, 27, 13, 30, 0, TimeSpan.Zero);
        var service = new HealthDiagnosticsService(healthService, new FixedTimeProvider(agora));

        var resultado = await service.ObterDetalhesAsync(CancellationToken.None);

        Assert.Equal("Degraded", resultado.Status);
        Assert.Equal(agora, resultado.CheckedAt);
        Assert.Equal(2, resultado.Checks.Count);
        Assert.Equal(8, resultado.Checks.Single(check => check.Name == "sqlite").DurationMs);
        Assert.DoesNotContain("exposto", resultado.Checks.SelectMany(check => new[] { check.Name, check.Status }));
    }

    [Fact]
    public async Task ObterDetalhes_deve_expor_uptime_coerente()
    {
        var service = new HealthDiagnosticsService(
            new HealthCheckServiceFake(new Dictionary<string, HealthReportEntry>()),
            new FixedTimeProvider(DateTimeOffset.UtcNow));

        var resultado = await service.ObterDetalhesAsync(CancellationToken.None);

        Assert.InRange(resultado.UptimeSeconds, 0, 5);
    }

    private sealed class HealthCheckServiceFake(IReadOnlyDictionary<string, HealthReportEntry> entries) : HealthCheckService
    {
        public override Task<HealthReport> CheckHealthAsync(
            Func<HealthCheckRegistration, bool>? predicate,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new HealthReport(entries, entries.Values.Aggregate(TimeSpan.Zero, (total, entry) => total + entry.Duration)));
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
