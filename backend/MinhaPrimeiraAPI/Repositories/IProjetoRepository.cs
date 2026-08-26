using ProjetoTarefas.Models;

namespace ProjetoTarefas.Repositories;

public interface IProjetoRepository
{
    Task<List<Projeto>> ListarAsync(CancellationToken cancellationToken = default);
    Task<Projeto?> BuscarPorNomeNormalizadoAsync(string nomeNormalizado, CancellationToken cancellationToken = default);
    Task<Projeto?> BuscarPorIdAsync(int id, bool rastrearAlteracoes = false, CancellationToken cancellationToken = default);
    void Adicionar(Projeto projeto);
    void Remover(Projeto projeto);
    Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
