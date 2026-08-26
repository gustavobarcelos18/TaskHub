using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ProjetoTarefas.DTOs.Requests;
using ProjetoTarefas.Models;
using ProjetoTarefas.Repositories;
using ProjetoTarefas.Services;
using ProjetoTarefas.Tests.Fakes;
using ProjetoTarefas.Tests.Infrastructure;

namespace ProjetoTarefas.Tests.Repositories;

public sealed class EtiquetaRepositoryTests
{
    [Fact]
    public async Task Repositorios_DevemRestringirTarefasProjetosEtiquetasEHistoricoAoUsuarioAtual()
    {
        await using var database = await CriarBancoAsync();
        const string usuarioAId = "usuario-a";
        const string usuarioBId = "usuario-b";

        await using (var gravacao = database.CreateContext())
        {
            gravacao.Users.AddRange(
                NovoUsuario(usuarioAId, "usuario-a@teste.local"),
                NovoUsuario(usuarioBId, "usuario-b@teste.local"));

            var projetoA = new Projeto { UsuarioId = usuarioAId, Nome = "Projeto A", NomeNormalizado = "PROJETO A" };
            var projetoB = new Projeto { UsuarioId = usuarioBId, Nome = "Projeto B", NomeNormalizado = "PROJETO B" };
            var etiquetaA = new Etiqueta { UsuarioId = usuarioAId, Nome = "Etiqueta A", NomeNormalizado = "ETIQUETA A" };
            var etiquetaB = new Etiqueta { UsuarioId = usuarioBId, Nome = "Etiqueta B", NomeNormalizado = "ETIQUETA B" };
            var agora = DateTime.UtcNow;
            var tarefaA = NovaTarefa("Tarefa A", agora); tarefaA.UsuarioId = usuarioAId; tarefaA.Projeto = projetoA; tarefaA.Etiquetas.Add(etiquetaA);
            var tarefaB = NovaTarefa("Tarefa B", agora); tarefaB.UsuarioId = usuarioBId; tarefaB.Projeto = projetoB; tarefaB.Etiquetas.Add(etiquetaB);
            gravacao.AddRange(tarefaA, tarefaB);
            gravacao.HistoricosTarefas.AddRange(
                new HistoricoTarefa { Tarefa = tarefaA, Tipo = TiposHistoricoTarefa.Criacao, CriadoEm = agora },
                new HistoricoTarefa { Tarefa = tarefaB, Tipo = TiposHistoricoTarefa.Criacao, CriadoEm = agora });
            await gravacao.SaveChangesAsync();
        }

        await using var leitura = database.CreateContext();
        var usuarioAtual = new UsuarioAtualFake(usuarioAId);
        var tarefas = new TarefaRepository(leitura, usuarioAtual);
        var projetos = new ProjetoRepository(leitura, usuarioAtual);
        var etiquetas = new EtiquetaRepository(leitura, usuarioAtual);

        var listaTarefas = await tarefas.ListarAtivasAsync(new ConsultaTarefas { Hoje = DateOnly.FromDateTime(DateTime.UtcNow), Pagina = 1, TamanhoPagina = 10 });
        var tarefaAEncontrada = Assert.Single(listaTarefas.Itens);

        Assert.Equal("Tarefa A", tarefaAEncontrada.Descricao);
        Assert.Equal(["Projeto A"], (await projetos.ListarAsync()).Select(projeto => projeto.Nome));
        Assert.Equal(["Etiqueta A"], (await etiquetas.ListarAsync()).Select(etiqueta => etiqueta.Nome));
        Assert.Single(await tarefas.ListarHistoricoAsync(tarefaAEncontrada.Id));
        Assert.Null(await tarefas.BuscarAtivaPorIdAsync(tarefaAEncontrada.Id + 1));
    }

    [Fact]
    public async Task Etiquetas_DevemSerUnicasPorNomeNormalizadoEOrdenadasNoSQLite()
    {
        await using var database = await CriarBancoAsync();
        await using (var context = database.CreateContext())
        {
            var usuarioAtual = new UsuarioAtualFake();
            context.Users.Add(NovoUsuario(usuarioAtual.Id, "usuario-teste@local"));
            await context.SaveChangesAsync();
            var service = new EtiquetaService(new EtiquetaRepository(context, usuarioAtual), usuarioAtual);
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

    [Fact]
    public async Task Projeto_DeveSerUnicoOrdenadoESeuDeleteDeveManterTarefasSemProjetoNoSQLite()
    {
        await using var database = await CriarBancoAsync();
        await using (var gravacao = database.CreateContext())
        {
            var usuarioAtual = new UsuarioAtualFake();
            gravacao.Users.Add(NovoUsuario(usuarioAtual.Id, "usuario-teste@local"));
            await gravacao.SaveChangesAsync();
            var projetos = new ProjetoRepository(gravacao, usuarioAtual);
            var service = new ProjetoService(projetos, usuarioAtual);
            var infraestrutura = await service.CriarAsync(new() { Nome = " Infraestrutura " });
            await service.CriarAsync(new() { Nome = "Financeiro" });
            await Assert.ThrowsAsync<ProjetoDuplicadoException>(() => service.CriarAsync(new() { Nome = "infraestrutura" }));
            var tarefa = NovaTarefa("Configurar firewall", DateTime.UtcNow);
            tarefa.ProjetoId = infraestrutura.Id;
            gravacao.Tarefas.Add(tarefa);
            await gravacao.SaveChangesAsync();
            Assert.True(await service.ExcluirAsync(infraestrutura.Id));
        }

        await using var verificacao = database.CreateContext();
        Assert.Equal(["Financeiro"], (await new ProjetoRepository(verificacao).ListarAsync()).Select(projeto => projeto.Nome));
        var tarefaPersistida = await verificacao.Tarefas.SingleAsync();
        Assert.Null(tarefaPersistida.ProjetoId);
    }

    [Fact]
    public async Task ProjetoEFiltroEtiqueta_DevemSerAplicadosAntesDaPaginacaoNoSQLite()
    {
        await using var database = await CriarBancoAsync();
        var agora = DateTime.UtcNow;
        await using (var gravacao = database.CreateContext())
        {
            var projetoA = new Projeto { Nome = "A", NomeNormalizado = "A" };
            var projetoB = new Projeto { Nome = "B", NomeNormalizado = "B" };
            var etiqueta = new Etiqueta { Nome = "X", NomeNormalizado = "X" };
            var primeira = NovaTarefa("Primeira", agora); primeira.Projeto = projetoA; primeira.Etiquetas.Add(etiqueta);
            var segunda = NovaTarefa("Segunda", agora); segunda.Projeto = projetoA;
            var terceira = NovaTarefa("Terceira", agora); terceira.Projeto = projetoB; terceira.Etiquetas.Add(etiqueta);
            gravacao.AddRange(primeira, segunda, terceira);
            await gravacao.SaveChangesAsync();
        }

        await using var leitura = database.CreateContext();
        var projeto = await leitura.Projetos.SingleAsync(item => item.Nome == "A");
        var etiquetaFiltro = await leitura.Etiquetas.SingleAsync();
        var resultado = await new TarefaRepository(leitura).ListarAtivasAsync(new ConsultaTarefas { ProjetoId = projeto.Id, EtiquetaId = etiquetaFiltro.Id, Hoje = DateOnly.FromDateTime(agora), OrdenarPor = CampoOrdenacaoTarefa.Descricao, Direcao = DirecaoOrdenacao.Asc, Pagina = 1, TamanhoPagina = 1 });
        var encontrada = Assert.Single(resultado.Itens);
        Assert.Equal(1, resultado.TotalItens);
        Assert.Equal("Primeira", encontrada.Descricao);
        Assert.Equal("A", encontrada.Projeto?.Nome);
    }

    [Fact]
    public async Task CriarTarefaComProjetoExistenteEEtiqueta_NaoDeveInserirONovoProjeto()
    {
        await using var database = await CriarBancoAsync();
        await using (var gravacao = database.CreateContext())
        {
            var usuarioAtual = new UsuarioAtualFake();
            gravacao.Users.Add(NovoUsuario(usuarioAtual.Id, "usuario-teste@local"));
            var projeto = new Projeto { UsuarioId = usuarioAtual.Id, Nome = "Projeto existente", NomeNormalizado = "PROJETO EXISTENTE" };
            var etiqueta = new Etiqueta { UsuarioId = usuarioAtual.Id, Nome = "Etiqueta existente", NomeNormalizado = "ETIQUETA EXISTENTE" };
            gravacao.AddRange(projeto, etiqueta);
            await gravacao.SaveChangesAsync();

            var service = new TarefaService(
                new TarefaRepository(gravacao, usuarioAtual),
                NullLogger<TarefaService>.Instance,
                TimeProvider.System,
                new EtiquetaRepository(gravacao, usuarioAtual),
                new ProjetoRepository(gravacao, usuarioAtual),
                usuarioAtual: usuarioAtual);

            var resultado = await service.CriarAsync(new CriarTarefaRequest
            {
                Descricao = "Tarefa com projeto existente",
                ProjetoId = projeto.Id,
                EtiquetaIds = [etiqueta.Id]
            });

            Assert.Equal(projeto.Id, resultado.Projeto?.Id);
            Assert.Equal(etiqueta.Id, Assert.Single(resultado.Etiquetas).Id);
        }

        await using var verificacao = database.CreateContext();
        Assert.Equal(1, await verificacao.Projetos.CountAsync());
        var tarefa = await verificacao.Tarefas.Include(item => item.Projeto).Include(item => item.Etiquetas).SingleAsync();
        Assert.Equal("Projeto existente", tarefa.Projeto?.Nome);
        Assert.Equal("Etiqueta existente", Assert.Single(tarefa.Etiquetas).Nome);
    }

    private static async Task<SqliteTestDatabase> CriarBancoAsync() { var database = new SqliteTestDatabase(); await database.InitializeAsync(); return database; }
    private static Tarefa NovaTarefa(string descricao, DateTime agora) => new() { Descricao = descricao, Situacao = SituacoesTarefa.Pendente, Prioridade = PrioridadesTarefa.Media, CriadaEm = agora, SituacaoAlteradaEm = agora };
    private static Usuario NovoUsuario(string id, string email) => new() { Id = id, UserName = email, NormalizedUserName = email.ToUpperInvariant(), Email = email, NormalizedEmail = email.ToUpperInvariant() };
}
