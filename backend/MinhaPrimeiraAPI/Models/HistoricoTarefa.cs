namespace MinhaPrimeiraAPI.Models;

public class HistoricoTarefa
{
    public int Id { get; set; }

    public int TarefaId { get; set; }

    public string Tipo { get; set; } = string.Empty;

    public string? Campo { get; set; }

    public string? ValorAnterior { get; set; }

    public string? ValorNovo { get; set; }

    public DateTime CriadoEm { get; set; }

    public Tarefa Tarefa { get; set; } = null!;
}
