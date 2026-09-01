namespace ProjetoTarefas.DTOs.Requests;

public sealed class ConsultaLogsRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
    public string? Level { get; init; }
    public string? User { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int? StatusCode { get; init; }
    public string? Method { get; init; }
    public string? Path { get; init; }
    public string? TraceId { get; init; }
    public string? Text { get; init; }
}
