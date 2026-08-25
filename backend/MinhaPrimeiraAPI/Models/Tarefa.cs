namespace MinhaPrimeiraAPI.Models;

public class Tarefa
{
    public int Id { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public string? Observacoes { get; set; }

    public string Situacao { get; set; } = SituacoesTarefa.Pendente;

    public string Prioridade { get; set; } = PrioridadesTarefa.Media;

    public DateOnly? DataVencimento { get; set; }

    public DateTime CriadaEm { get; set; }

    public DateTime? ModificadaEm { get; set; }

    public DateTime SituacaoAlteradaEm { get; set; }

    public DateTime? ConcluidaEm { get; set; }

    public DateTime? ExcluidaEm { get; set; }

    public int? ProjetoId { get; set; }

    public Projeto? Projeto { get; set; }

    public ICollection<Etiqueta> Etiquetas { get; set; } = new List<Etiqueta>();
}
