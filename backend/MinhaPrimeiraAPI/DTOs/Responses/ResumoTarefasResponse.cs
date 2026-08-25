namespace MinhaPrimeiraAPI.DTOs.Responses;

public sealed class ResumoTarefasResponse
{
    public int Total { get; init; }

    public int Pendentes { get; init; }

    public int EmAndamento { get; init; }

    public int Concluidas { get; init; }
}
