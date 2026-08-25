namespace MinhaPrimeiraAPI.DTOs.Responses;

public class HistoricoTarefaResponse
{
    public int Id { get; set; }

    public string Tipo { get; set; } = string.Empty;

    public string? Campo { get; set; }

    public string? ValorAnterior { get; set; }

    public string? ValorNovo { get; set; }

    public DateTime CriadoEm { get; set; }
}
