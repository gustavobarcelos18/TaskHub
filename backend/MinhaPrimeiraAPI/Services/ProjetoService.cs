using Microsoft.EntityFrameworkCore;
using ProjetoTarefas.DTOs.Requests;
using ProjetoTarefas.DTOs.Responses;
using ProjetoTarefas.Models;
using ProjetoTarefas.Repositories;

namespace ProjetoTarefas.Services;

public class ProjetoService(IProjetoRepository repository, IUsuarioAtual usuarioAtual) : IProjetoService
{
    public async Task<List<ProjetoResponse>> ListarAsync(CancellationToken cancellationToken = default) => (await repository.ListarAsync(cancellationToken)).Select(Mapear).ToList();
    public async Task<ProjetoResponse> CriarAsync(CriarProjetoRequest request, CancellationToken cancellationToken = default)
    {
        var nome = NormalizarNome(request.Nome);
        var normalizado = nome.ToUpperInvariant();
        if (await repository.BuscarPorNomeNormalizadoAsync(normalizado, cancellationToken) is not null) throw new ProjetoDuplicadoException();
        var projeto = new Projeto { UsuarioId = usuarioAtual.Id, Nome = nome, NomeNormalizado = normalizado };
        repository.Adicionar(projeto);
        try { await repository.SalvarAlteracoesAsync(cancellationToken); }
        catch (DbUpdateException) { throw new ProjetoDuplicadoException(); }
        return Mapear(projeto);
    }
    public async Task<bool> ExcluirAsync(int id, CancellationToken cancellationToken = default)
    {
        var projeto = await repository.BuscarPorIdAsync(id, true, cancellationToken);
        if (projeto is null) return false;
        repository.Remover(projeto);
        await repository.SalvarAlteracoesAsync(cancellationToken);
        return true;
    }
    public static string NormalizarNome(string? nome)
    {
        var resultado = nome?.Trim();
        if (string.IsNullOrWhiteSpace(resultado)) throw new ArgumentException("O nome do projeto é obrigatório.", nameof(nome));
        if (resultado.Length > 100) throw new ArgumentException("O nome do projeto deve ter no máximo 100 caracteres.", nameof(nome));
        return resultado;
    }
    private static ProjetoResponse Mapear(Projeto projeto) => new() { Id = projeto.Id, Nome = projeto.Nome };
}
