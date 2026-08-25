using MinhaPrimeiraAPI.Models;

namespace MinhaPrimeiraAPI.Repositories;

public sealed class ResultadoConsultaTarefas
{
    public List<Tarefa> Itens { get; init; } = [];
    public int TotalItens { get; init; }
}
