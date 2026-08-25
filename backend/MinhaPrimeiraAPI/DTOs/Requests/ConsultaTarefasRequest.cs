namespace MinhaPrimeiraAPI.DTOs.Requests;

public class ConsultaTarefasRequest
{
    public string? Busca { get; set; }
    public string? Situacao { get; set; }
    public string? Prioridade { get; set; }
    public string? Prazo { get; set; }
    public string? OrdenarPor { get; set; }
    public string? Direcao { get; set; }
    public int? Pagina { get; set; }
    public int? TamanhoPagina { get; set; }
}
