namespace MinhaPrimeiraAPI.Repositories;

public sealed class ConsultaTarefas
{
    public string? Busca { get; init; }
    public string? Situacao { get; init; }
    public string? Prioridade { get; init; }
    public int? EtiquetaId { get; init; }
    public int? ProjetoId { get; init; }
    public FiltroPrazoTarefa Prazo { get; init; }
    public DateOnly Hoje { get; init; }
    public CampoOrdenacaoTarefa OrdenarPor { get; init; }
    public DirecaoOrdenacao Direcao { get; init; }
    public int Pagina { get; init; }
    public int TamanhoPagina { get; init; }
}

public enum CampoOrdenacaoTarefa { Descricao, Situacao, Prioridade, DataVencimento, UltimaAtualizacao }
public enum DirecaoOrdenacao { Asc, Desc }
public enum FiltroPrazoTarefa { Todos, Vencidas, VencemHoje, Proximas, SemVencimento }
