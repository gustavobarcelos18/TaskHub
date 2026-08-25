using Microsoft.EntityFrameworkCore;
using MinhaPrimeiraAPI.Data;
using MinhaPrimeiraAPI.Models;

namespace MinhaPrimeiraAPI.Repositories;

public class EtiquetaRepository(AppDbContext context) : IEtiquetaRepository
{
    public Task<List<Etiqueta>> ListarAsync(CancellationToken cancellationToken = default) => context.Etiquetas.AsNoTracking().OrderBy(etiqueta => etiqueta.Nome).ThenBy(etiqueta => etiqueta.Id).ToListAsync(cancellationToken);
    public Task<Etiqueta?> BuscarPorNomeNormalizadoAsync(string nomeNormalizado, CancellationToken cancellationToken = default) => context.Etiquetas.AsNoTracking().SingleOrDefaultAsync(etiqueta => etiqueta.NomeNormalizado == nomeNormalizado, cancellationToken);
    public Task<List<Etiqueta>> BuscarPorIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default) => context.Etiquetas.Where(etiqueta => ids.Contains(etiqueta.Id)).ToListAsync(cancellationToken);
    public async Task<Etiqueta?> BuscarPorIdAsync(int id, bool rastrearAlteracoes = false, CancellationToken cancellationToken = default)
    {
        IQueryable<Etiqueta> consulta = context.Etiquetas;
        if (!rastrearAlteracoes) consulta = consulta.AsNoTracking();
        return await consulta.SingleOrDefaultAsync(etiqueta => etiqueta.Id == id, cancellationToken);
    }
    public void Adicionar(Etiqueta etiqueta) => context.Etiquetas.Add(etiqueta);
    public void Remover(Etiqueta etiqueta) => context.Etiquetas.Remove(etiqueta);
    public Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default) => context.SaveChangesAsync(cancellationToken);
}
