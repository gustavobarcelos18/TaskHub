namespace ProjetoTarefas.DTOs.Responses;

public sealed class LogsPaginadosResponse
{
    public List<LogEventoResponse> Itens { get; init; } = [];
    public int PaginaAtual { get; init; }
    public int TamanhoPagina { get; init; }
    public int TotalItens { get; init; }
    public int TotalPaginas { get; init; }
}
