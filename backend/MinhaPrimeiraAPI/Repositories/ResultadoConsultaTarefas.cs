using ProjetoTarefas.Models;

namespace ProjetoTarefas.Repositories;

public sealed class ResultadoConsultaTarefas
{
    public List<Tarefa> Itens { get; init; } = [];
    public int TotalItens { get; init; }
}
