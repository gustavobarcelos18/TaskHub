using Microsoft.EntityFrameworkCore;
using MinhaPrimeiraAPI.Models;
using MinhaPrimeiraAPI.Repositories;
using MinhaPrimeiraAPI.Services;
using MinhaPrimeiraAPI.Tests.Infrastructure;

namespace MinhaPrimeiraAPI.Tests.Repositories;

public sealed class EtiquetaRepositoryTests
{
    [Fact]
    public async Task Etiquetas_DevemSerUnicasPorNomeNormalizadoEOrdenadasNoSQLite()
    {
        await using var database = await CriarBancoAsync();
        await using (var context = database.CreateContext())
        {
            var service = new EtiquetaService(new EtiquetaRepository(context));
            var urgente = await service.CriarAsync(new() { Nome = " Urgente " });
            Assert.Equal("Urgente", urgente.Nome);
            await Assert.ThrowsAsync<EtiquetaDuplicadaException>(() => service.CriarAsync(new() { Nome = "urgente" }));
            await service.CriarAsync(new() { Nome = "Financeiro" });
        }

        await using var leitura = database.CreateContext();
        var etiquetas = await new EtiquetaRepository(leitura).ListarAsync();
        Assert.Equal(["Financeiro", "Urgente"], etiquetas.Select(etiqueta => etiqueta.Nome));
    }

    [Fact]
    public async Task RelacaoNParaN_DeveFiltrarAntesDaPaginacaoECascatearSomenteAssociacoes()
    {
        await using var database = await CriarBancoAsync();
        var agora = DateTime.UtcNow;
        await using (var gravacao = database.CreateContext())
        {
            var x = new Etiqueta { Nome = "X", NomeNormalizado = "X" };
            var y = new Etiqueta { Nome = "Y", NomeNormalizado = "Y" };
            var a = NovaTarefa("A", agora); a.Etiquetas.Add(x);
            var b = NovaTarefa("B", agora); b.Etiquetas.Add(y);
            var c = NovaTarefa("C", agora); c.Etiquetas.Add(x); c.Etiquetas.Add(y);
            gravacao.AddRange(a, b, c);
            await gravacao.SaveChangesAsync();
        }

        await using (var leitura = database.CreateContext())
        {
            var x = await leitura.Etiquetas.SingleAsync(etiqueta => etiqueta.Nome == "X");
            var resultado = await new TarefaRepository(leitura).ListarAtivasAsync(new ConsultaTarefas { EtiquetaId = x.Id, Hoje = DateOnly.FromDateTime(agora), OrdenarPor = CampoOrdenacaoTarefa.Descricao, Direcao = DirecaoOrdenacao.Asc, Pagina = 1, TamanhoPagina = 1 });
            Assert.Equal(2, resultado.TotalItens);
            Assert.Equal("A", Assert.Single(resultado.Itens).Descricao);
            Assert.Single(resultado.Itens[0].Etiquetas);
        }

        await using (var exclusao = database.CreateContext())
        {
            var x = await exclusao.Etiquetas.SingleAsync(etiqueta => etiqueta.Nome == "X");
            exclusao.Etiquetas.Remove(x);
            await exclusao.SaveChangesAsync();
        }

        await using var verificacao = database.CreateContext();
        Assert.Equal(3, await verificacao.Tarefas.CountAsync());
        Assert.Equal(1, await verificacao.Etiquetas.CountAsync());
        Assert.Equal(2, await verificacao.Set<Dictionary<string, object>>("TarefaEtiqueta").CountAsync());
    }

    private static async Task<SqliteTestDatabase> CriarBancoAsync() { var database = new SqliteTestDatabase(); await database.InitializeAsync(); return database; }
    private static Tarefa NovaTarefa(string descricao, DateTime agora) => new() { Descricao = descricao, Situacao = SituacoesTarefa.Pendente, Prioridade = PrioridadesTarefa.Media, CriadaEm = agora, SituacaoAlteradaEm = agora };
}
