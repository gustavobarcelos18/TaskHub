namespace MinhaPrimeiraAPI.Repositories;

public sealed class ResultadoResumoTarefas
{
    public int Total { get; init; }

    public int Pendentes { get; init; }

    public int EmAndamento { get; init; }

    public int Concluidas { get; init; }
}
