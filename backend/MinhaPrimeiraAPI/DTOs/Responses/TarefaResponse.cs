namespace MinhaPrimeiraAPI.DTOs.Responses;

public class TarefaResponse
{
    public int Id { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public string Situacao { get; set; } = string.Empty;

    public DateTime CriadaEm { get; set; }

    public DateTime? ModificadaEm { get; set; }

    public DateTime SituacaoAlteradaEm { get; set; }

    public DateTime? ConcluidaEm { get; set; }

    public DateTime? ExcluidaEm { get; set; }
}