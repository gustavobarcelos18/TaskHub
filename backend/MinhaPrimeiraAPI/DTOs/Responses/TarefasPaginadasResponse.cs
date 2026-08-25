namespace MinhaPrimeiraAPI.DTOs.Responses;

public class TarefasPaginadasResponse
{
    public List<TarefaResponse> Itens { get; set; } = [];
    public int PaginaAtual { get; set; }
    public int TamanhoPagina { get; set; }
    public int TotalItens { get; set; }
    public int TotalPaginas { get; set; }
}
