using MinhaPrimeiraAPI.Models;

namespace MinhaPrimeiraAPI.Repositories;

public interface ITarefaRepository
{
    Task<List<Tarefa>> ListarAtivasAsync();

    Task<Tarefa?> BuscarAtivaPorIdAsync(
        int id,
        bool rastrearAlteracoes = false
    );

    Task<Tarefa?> BuscarIncluindoExcluidasPorIdAsync(int id);

    void Adicionar(Tarefa tarefa);

    void Remover(Tarefa tarefa);

    Task<int> SalvarAlteracoesAsync();
}
