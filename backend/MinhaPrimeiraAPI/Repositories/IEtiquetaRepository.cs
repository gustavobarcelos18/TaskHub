using MinhaPrimeiraAPI.Models;

namespace MinhaPrimeiraAPI.Repositories;

public interface IEtiquetaRepository
{
    Task<List<Etiqueta>> ListarAsync(CancellationToken cancellationToken = default);
    Task<Etiqueta?> BuscarPorNomeNormalizadoAsync(string nomeNormalizado, CancellationToken cancellationToken = default);
    Task<List<Etiqueta>> BuscarPorIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default);
    Task<Etiqueta?> BuscarPorIdAsync(int id, bool rastrearAlteracoes = false, CancellationToken cancellationToken = default);
    void Adicionar(Etiqueta etiqueta);
    void Remover(Etiqueta etiqueta);
    Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
