namespace ProjetoTarefas.DTOs.Responses;

public sealed class HealthDetalhadoResponse
{
    public required string Status { get; init; }
    public required long UptimeSeconds { get; init; }
    public required DateTimeOffset CheckedAt { get; init; }
    public required IReadOnlyList<HealthCheckDetalhadoResponse> Checks { get; init; }
}

public sealed class HealthCheckDetalhadoResponse
{
    public required string Name { get; init; }
    public required string Status { get; init; }
    public required double DurationMs { get; init; }
}
