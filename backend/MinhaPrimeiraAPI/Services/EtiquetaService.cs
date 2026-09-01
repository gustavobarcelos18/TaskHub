using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProjetoTarefas.DTOs.Requests;
using ProjetoTarefas.DTOs.Responses;
using ProjetoTarefas.Models;
using ProjetoTarefas.Repositories;

namespace ProjetoTarefas.Services;

public class EtiquetaService(IEtiquetaRepository repository, IUsuarioAtual usuarioAtual) : IEtiquetaService
{
    public async Task<List<EtiquetaResponse>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var etiquetas = await repository.ListarAsync(cancellationToken);
        return etiquetas.Select(Mapear).ToList();
    }

    public async Task<EtiquetaResponse> CriarAsync(CriarEtiquetaRequest request, CancellationToken cancellationToken = default)
    {
        var nome = NormalizarNome(request.Nome);
        var normalizado = nome.ToUpperInvariant();

        if (await repository.BuscarPorNomeNormalizadoAsync(normalizado, cancellationToken) is not null)
        {
            throw new EtiquetaDuplicadaException();
        }

        var etiqueta = new Etiqueta { UsuarioId = usuarioAtual.Id, Nome = nome, NomeNormalizado = normalizado };
        repository.Adicionar(etiqueta);

        try
        {
            await repository.SalvarAlteracoesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (EhViolacaoDeUnicidade(exception))
        {
            throw new EtiquetaDuplicadaException();
        }

        return Mapear(etiqueta);
    }

    public async Task<bool> ExcluirAsync(int id, CancellationToken cancellationToken = default)
    {
        var etiqueta = await repository.BuscarPorIdAsync(id, true, cancellationToken);

        if (etiqueta is null)
        {
            return false;
        }

        repository.Remover(etiqueta);
        await repository.SalvarAlteracoesAsync(cancellationToken);
        return true;
    }

    public static string NormalizarNome(string? nome)
    {
        var resultado = nome?.Trim();

        if (string.IsNullOrWhiteSpace(resultado))
        {
            throw new ArgumentException("O nome da etiqueta é obrigatório.", nameof(nome));
        }

        if (resultado.Length > 50)
        {
            throw new ArgumentException("O nome da etiqueta deve ter no máximo 50 caracteres.", nameof(nome));
        }

        return resultado;
    }

    private static bool EhViolacaoDeUnicidade(DbUpdateException exception)
    {
        return exception.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 };
    }

    private static EtiquetaResponse Mapear(Etiqueta etiqueta) => new() { Id = etiqueta.Id, Nome = etiqueta.Nome };
}
