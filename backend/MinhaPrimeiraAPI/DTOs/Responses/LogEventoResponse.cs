namespace ProjetoTarefas.DTOs.Responses;

public sealed class LogEventoResponse
{
    public long Id { get; init; }
    public DateTime Timestamp { get; init; }
    public string Level { get; init; } = string.Empty;
    public string EventName { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? UserId { get; init; }
    public string? UserName { get; init; }
    public string? Method { get; init; }
    public string? Path { get; init; }
    public int? StatusCode { get; init; }
    public double? ElapsedMs { get; init; }
    public string? TraceId { get; init; }
    public Dictionary<string, string> Properties { get; init; } = [];
}
