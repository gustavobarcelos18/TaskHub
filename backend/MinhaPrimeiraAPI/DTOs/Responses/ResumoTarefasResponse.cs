namespace ProjetoTarefas.DTOs.Responses;

public sealed class ResumoTarefasResponse
{
    public int Total { get; init; }

    public int Pendentes { get; init; }

    public int EmAndamento { get; init; }

    public int Concluidas { get; init; }

    public int Vencidas { get; init; }

    public int VencemHoje { get; init; }

    public int Proximas { get; init; }
}
