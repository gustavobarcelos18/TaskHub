using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging.Abstractions;
using ProjetoTarefas.DTOs.Requests;
using ProjetoTarefas.Models;
using ProjetoTarefas.Repositories;
using ProjetoTarefas.Services;
using ProjetoTarefas.Tests.Fakes;
using Xunit;

namespace ProjetoTarefas.Tests.Services;

public sealed class TarefaServiceTests
{
    private static readonly DateTimeOffset InstanteControlado = new(2030, 1, 2, 3, 4, 5, TimeSpan.Zero);

    [Fact]
    public async Task CriarAsync_DeveUsarInstanteControladoNasDatasDeAuditoria()
    {
        var repository = new TarefaRepositoryFake();

        var resultado = await CriarService(repository).CriarAsync(new CriarTarefaRequest
        {
            Descricao = "Tarefa controlada",
            Situacao = "Concluída"
        });

        Assert.Equal(InstanteControlado.UtcDateTime, resultado.CriadaEm);
        Assert.Equal(InstanteControlado.UtcDateTime, resultado.SituacaoAlteradaEm);
        Assert.Equal(InstanteControlado.UtcDateTime, resultado.ConcluidaEm);
        Assert.Null(resultado.ModificadaEm);
        Assert.Null(resultado.ExcluidaEm);
    }

    [Fact]
    public async Task AtualizarAsync_DeveUsarInstanteControladoNasDatasAlteradas()
    {
        var tarefa = CriarTarefa(500, "Original", "Pendente");
        var repository = new TarefaRepositoryFake { TarefaRetornadaPorBuscaAtiva = tarefa };

        var resultado = await CriarService(repository).AtualizarAsync(tarefa.Id, new AtualizarTarefaRequest
        {
            Descricao = "Atualizada",
            Situacao = "Concluída"
        });

        Assert.NotNull(resultado);
        Assert.Equal(InstanteControlado.UtcDateTime, resultado.ModificadaEm);
        Assert.Equal(InstanteControlado.UtcDateTime, resultado.SituacaoAlteradaEm);
        Assert.Equal(InstanteControlado.UtcDateTime, resultado.ConcluidaEm);
    }

    [Fact]
    public async Task ListarAsync_DeveRepassarCancellationTokenAoRepositorio()
    {
        var repository = new TarefaRepositoryFake();
        using var source = new CancellationTokenSource();

        await CriarService(repository).ListarAsync(new ConsultaTarefasRequest(), source.Token);

        Assert.Equal(source.Token, repository.UltimoCancellationToken);
    }
    [Fact]
    public async Task ObterResumoAsync_DeveMapearOsQuatroContadoresDoRepository()
    {
        // Arrange
        var tarefaRepository = new TarefaRepositoryFake
        {
            ResultadoResumoConfigurado = new ResultadoResumoTarefas
            {
                Total = 10,
                Pendentes = 3,
                EmAndamento = 2,
                Concluidas = 4
            }
        };

        var tarefaService = CriarService(tarefaRepository);

        // Act
        var resultado = await tarefaService.ObterResumoAsync();

        // Assert
        Assert.Equal(10, resultado.Total);
        Assert.Equal(3, resultado.Pendentes);
        Assert.Equal(2, resultado.EmAndamento);
        Assert.Equal(4, resultado.Concluidas);
        Assert.Equal(1, tarefaRepository.QuantidadeChamadasObterResumoAtivas);
    }

    [Fact]
    public async Task ObterResumoAsync_RepositoryComZeros_DeveRetornarQuatroZeros()
    {
        // Arrange
        var tarefaRepository = new TarefaRepositoryFake();
        var tarefaService = CriarService(tarefaRepository);

        // Act
        var resultado = await tarefaService.ObterResumoAsync();

        // Assert
        Assert.Equal(0, resultado.Total);
        Assert.Equal(0, resultado.Pendentes);
        Assert.Equal(0, resultado.EmAndamento);
        Assert.Equal(0, resultado.Concluidas);
    }

    [Fact]
    public async Task ListarAsync_DeveRetornarTodasAsTarefasAtivasMapeadas()
    {
        // Arrange
        var primeiraTarefa = CriarTarefa(
            id: 1,
            descricao: "Comprar ração",
            situacao: "Pendente"
        );

        var segundaTarefa = CriarTarefa(
            id: 2,
            descricao: "Estudar xUnit",
            situacao: "Em andamento"
        );

        var tarefaRepository = new TarefaRepositoryFake();

        tarefaRepository.TarefasAtivas.Add(primeiraTarefa);
        tarefaRepository.TarefasAtivas.Add(segundaTarefa);

        var tarefaService = CriarService(tarefaRepository);

        // Act
        var resultado = await tarefaService.ListarAsync(new ConsultaTarefasRequest());

        // Assert
        Assert.Equal(2, resultado.Itens.Count);
        Assert.Equal(1, resultado.PaginaAtual);
        Assert.Equal(10, resultado.TamanhoPagina);
        Assert.Equal(2, resultado.TotalItens);
        Assert.Equal(1, resultado.TotalPaginas);

        Assert.Equal(primeiraTarefa.Id, resultado.Itens[0].Id);
        Assert.Equal(primeiraTarefa.Descricao, resultado.Itens[0].Descricao);
        Assert.Equal(primeiraTarefa.Situacao, resultado.Itens[0].Situacao);
        Assert.Equal(primeiraTarefa.CriadaEm, resultado.Itens[0].CriadaEm);
        Assert.Equal(primeiraTarefa.ModificadaEm, resultado.Itens[0].ModificadaEm);
        Assert.Equal(
            primeiraTarefa.SituacaoAlteradaEm,
            resultado.Itens[0].SituacaoAlteradaEm
        );
        Assert.Equal(
            primeiraTarefa.ConcluidaEm,
            resultado.Itens[0].ConcluidaEm
        );
        Assert.Equal(
            primeiraTarefa.ExcluidaEm,
            resultado.Itens[0].ExcluidaEm
        );

        Assert.Equal(segundaTarefa.Id, resultado.Itens[1].Id);
        Assert.Equal(segundaTarefa.Descricao, resultado.Itens[1].Descricao);
        Assert.Equal(segundaTarefa.Situacao, resultado.Itens[1].Situacao);

        Assert.Equal(
            1,
            tarefaRepository.QuantidadeChamadasListarAtivas
        );
    }

    [Fact]
    public async Task BuscarPorIdAsync_TarefaExistente_DeveRetornarTarefaMapeada()
    {
        // Arrange
        var tarefa = CriarTarefa(
            id: 10,
            descricao: "Preparar relatório",
            situacao: "Em andamento"
        );

        var tarefaRepository = new TarefaRepositoryFake
        {
            TarefaRetornadaPorBuscaAtiva = tarefa
        };

        var tarefaService = CriarService(tarefaRepository);

        // Act
        var resultado = await tarefaService.BuscarPorIdAsync(tarefa.Id);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(tarefa.Id, resultado.Id);
        Assert.Equal(tarefa.Descricao, resultado.Descricao);
        Assert.Equal(tarefa.Situacao, resultado.Situacao);
        Assert.Equal(tarefa.CriadaEm, resultado.CriadaEm);
        Assert.Equal(tarefa.ModificadaEm, resultado.ModificadaEm);
        Assert.Equal(tarefa.SituacaoAlteradaEm, resultado.SituacaoAlteradaEm);
        Assert.Equal(tarefa.ConcluidaEm, resultado.ConcluidaEm);
        Assert.Equal(tarefa.ExcluidaEm, resultado.ExcluidaEm);

        Assert.Equal(tarefa.Id, tarefaRepository.UltimoIdBuscadoAtiva);
        Assert.False(tarefaRepository.UltimaBuscaAtivaRastreouAlteracoes);
    }

    [Fact]
    public async Task BuscarPorIdAsync_TarefaInexistente_DeveRetornarNull()
    {
        // Arrange
        var tarefaRepository = new TarefaRepositoryFake
        {
            TarefaRetornadaPorBuscaAtiva = null
        };

        var tarefaService = CriarService(tarefaRepository);

        const int tarefaId = 999;

        // Act
        var resultado = await tarefaService.BuscarPorIdAsync(tarefaId);

        // Assert
        Assert.Null(resultado);
        Assert.Equal(tarefaId, tarefaRepository.UltimoIdBuscadoAtiva);
        Assert.False(tarefaRepository.UltimaBuscaAtivaRastreouAlteracoes);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CriarAsync_SituacaoAusente_DeveCriarComoPendente(
        string? situacao)
    {
        // Arrange
        var tarefaRepository = new TarefaRepositoryFake();
        var tarefaService = CriarService(tarefaRepository);

        var request = new CriarTarefaRequest
        {
            Descricao = "Comprar ração",
            Situacao = situacao
        };

        var instanteAnterior = DateTime.UtcNow;

        // Act
        var resultado = await tarefaService.CriarAsync(request);

        var instantePosterior = DateTime.UtcNow;

        // Assert
        Assert.Equal("Comprar ração", resultado.Descricao);
        Assert.Equal("Pendente", resultado.Situacao);

        Assert.Equal(InstanteControlado.UtcDateTime, resultado.CriadaEm);

        Assert.Equal(
            resultado.CriadaEm,
            resultado.SituacaoAlteradaEm
        );

        Assert.Null(resultado.ModificadaEm);
        Assert.Null(resultado.ConcluidaEm);
        Assert.Null(resultado.ExcluidaEm);

        Assert.Same(
            tarefaRepository.TarefaAdicionada,
            tarefaRepository.TarefaAdicionada
        );

        Assert.Equal(
            1,
            tarefaRepository.QuantidadeChamadasAdicionar
        );

        Assert.Equal(
            1,
            tarefaRepository.QuantidadeChamadasSalvarAlteracoes
        );
    }

    [Fact]
    public async Task CriarAsync_DescricaoESituacaoComEspacos_DeveAplicarTrim()
    {
        // Arrange
        var tarefaRepository = new TarefaRepositoryFake();
        var tarefaService = CriarService(tarefaRepository);

        var request = new CriarTarefaRequest
        {
            Descricao = "   Comprar ração   ",
            Situacao = "   Em andamento   "
        };

        // Act
        var resultado = await tarefaService.CriarAsync(request);

        // Assert
        Assert.Equal("Comprar ração", resultado.Descricao);
        Assert.Equal("Em andamento", resultado.Situacao);

        Assert.NotNull(tarefaRepository.TarefaAdicionada);

        Assert.Equal(
            "Comprar ração",
            tarefaRepository.TarefaAdicionada.Descricao
        );

        Assert.Equal(
            "Em andamento",
            tarefaRepository.TarefaAdicionada.Situacao
        );

        Assert.Null(resultado.ConcluidaEm);
    }

    [Theory]
    [InlineData("Concluída")]
    [InlineData("concluída")]
    [InlineData("CONCLUÍDA")]
    public async Task CriarAsync_TarefaConcluida_DevePreencherConcluidaEm(
        string situacao)
    {
        // Arrange
        var tarefaRepository = new TarefaRepositoryFake();
        var tarefaService = CriarService(tarefaRepository);

        var request = new CriarTarefaRequest
        {
            Descricao = "Finalizar atividade",
            Situacao = situacao
        };

        var instanteAnterior = DateTime.UtcNow;

        // Act
        var resultado = await tarefaService.CriarAsync(request);

        var instantePosterior = DateTime.UtcNow;

        // Assert
        Assert.NotNull(resultado.ConcluidaEm);

        Assert.Equal(InstanteControlado.UtcDateTime, resultado.ConcluidaEm);

        Assert.Equal(
            resultado.CriadaEm,
            resultado.ConcluidaEm
        );

        Assert.Equal(
            resultado.SituacaoAlteradaEm,
            resultado.ConcluidaEm
        );

        Assert.Null(resultado.ModificadaEm);
        Assert.Null(resultado.ExcluidaEm);
    }

    [Fact]
    public async Task CriarAsync_SituacaoComCapitalizacaoDiferente_DeveSalvarValorCanonico()
    {
        // Arrange
        var tarefaRepository = new TarefaRepositoryFake();
        var tarefaService = CriarService(tarefaRepository);

        var request = new CriarTarefaRequest
        {
            Descricao = "Finalizar atividade",
            Situacao = "concluída"
        };

        // Act
        var resultado = await tarefaService.CriarAsync(request);

        // Assert
        Assert.Equal("Concluída", resultado.Situacao);
        Assert.NotNull(resultado.ConcluidaEm);
    }

    [Fact]
    public async Task CriarAsync_SituacaoInvalida_NaoDevePersistir()
    {
        // Arrange
        var tarefaRepository = new TarefaRepositoryFake();
        var tarefaService = CriarService(tarefaRepository);

        var request = new CriarTarefaRequest
        {
            Descricao = "Finalizar atividade",
            Situacao = "Cancelada"
        };

        // Act
        var excecao = await Assert.ThrowsAsync<ArgumentException>(
            () => tarefaService.CriarAsync(request)
        );

        // Assert
        Assert.Equal("situacao", excecao.ParamName);
        Assert.Equal(0, tarefaRepository.QuantidadeChamadasAdicionar);
        Assert.Equal(0, tarefaRepository.QuantidadeChamadasSalvarAlteracoes);
    }

    [Fact]
    public async Task AtualizarAsync_TarefaInexistente_DeveRetornarNullENaoSalvar()
    {
        // Arrange
        var tarefaRepository = new TarefaRepositoryFake
        {
            TarefaRetornadaPorBuscaAtiva = null
        };

        var tarefaService = CriarService(tarefaRepository);

        var request = new AtualizarTarefaRequest
        {
            Descricao = "Nova descrição",
            Situacao = "Em andamento"
        };

        const int tarefaId = 999;

        // Act
        var resultado = await tarefaService.AtualizarAsync(
            tarefaId,
            request
        );

        // Assert
        Assert.Null(resultado);
        Assert.Equal(tarefaId, tarefaRepository.UltimoIdBuscadoAtiva);
        Assert.True(tarefaRepository.UltimaBuscaAtivaRastreouAlteracoes);

        Assert.Equal(
            0,
            tarefaRepository.QuantidadeChamadasSalvarAlteracoes
        );
    }

    [Fact]
    public async Task AtualizarAsync_SemAlteracoes_NaoDeveSalvarNemAlterarDatas()
    {
        // Arrange
        var criadaEm = DateTime.UtcNow.AddDays(-3);
        var situacaoAlteradaEm = DateTime.UtcNow.AddDays(-2);

        var tarefa = new Tarefa
        {
            Id = 20,
            Descricao = "Comprar ração",
            Situacao = "Pendente",
            CriadaEm = criadaEm,
            ModificadaEm = null,
            SituacaoAlteradaEm = situacaoAlteradaEm,
            ConcluidaEm = null,
            ExcluidaEm = null
        };

        var tarefaRepository = new TarefaRepositoryFake
        {
            TarefaRetornadaPorBuscaAtiva = tarefa
        };

        var tarefaService = CriarService(tarefaRepository);

        var request = new AtualizarTarefaRequest
        {
            Descricao = "   Comprar ração   ",
            Situacao = "   Pendente   "
        };

        // Act
        var resultado = await tarefaService.AtualizarAsync(
            tarefa.Id,
            request
        );

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Comprar ração", resultado.Descricao);
        Assert.Equal("Pendente", resultado.Situacao);
        Assert.Null(resultado.ModificadaEm);
        Assert.Equal(situacaoAlteradaEm, resultado.SituacaoAlteradaEm);
        Assert.Null(resultado.ConcluidaEm);

        Assert.Equal(
            0,
            tarefaRepository.QuantidadeChamadasSalvarAlteracoes
        );
    }

    [Fact]
    public async Task AtualizarAsync_ApenasDescricao_DeveAlterarModificadaEm()
    {
        // Arrange
        var criadaEm = DateTime.UtcNow.AddDays(-3);
        var situacaoAlteradaEm = DateTime.UtcNow.AddDays(-2);

        var tarefa = new Tarefa
        {
            Id = 30,
            Descricao = "Descrição antiga",
            Situacao = "Pendente",
            CriadaEm = criadaEm,
            ModificadaEm = null,
            SituacaoAlteradaEm = situacaoAlteradaEm,
            ConcluidaEm = null,
            ExcluidaEm = null
        };

        var tarefaRepository = new TarefaRepositoryFake
        {
            TarefaRetornadaPorBuscaAtiva = tarefa
        };

        var tarefaService = CriarService(tarefaRepository);

        var request = new AtualizarTarefaRequest
        {
            Descricao = "Descrição atualizada",
            Situacao = "Pendente"
        };

        var instanteAnterior = DateTime.UtcNow;

        // Act
        var resultado = await tarefaService.AtualizarAsync(
            tarefa.Id,
            request
        );

        var instantePosterior = DateTime.UtcNow;

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Descrição atualizada", resultado.Descricao);
        Assert.Equal("Pendente", resultado.Situacao);

        Assert.NotNull(resultado.ModificadaEm);

        Assert.Equal(InstanteControlado.UtcDateTime, resultado.ModificadaEm);

        Assert.Equal(
            situacaoAlteradaEm,
            resultado.SituacaoAlteradaEm
        );

        Assert.Null(resultado.ConcluidaEm);

        Assert.Equal(
            1,
            tarefaRepository.QuantidadeChamadasSalvarAlteracoes
        );
    }

    [Fact]
    public async Task AtualizarAsync_ConcluindoTarefa_DeveAtualizarDatas()
    {
        // Arrange
        var criadaEm = DateTime.UtcNow.AddDays(-3);
        var situacaoAlteradaEm = DateTime.UtcNow.AddDays(-2);

        var tarefa = new Tarefa
        {
            Id = 40,
            Descricao = "Finalizar relatório",
            Situacao = "Em andamento",
            CriadaEm = criadaEm,
            ModificadaEm = null,
            SituacaoAlteradaEm = situacaoAlteradaEm,
            ConcluidaEm = null,
            ExcluidaEm = null
        };

        var tarefaRepository = new TarefaRepositoryFake
        {
            TarefaRetornadaPorBuscaAtiva = tarefa
        };

        var tarefaService = CriarService(tarefaRepository);

        var request = new AtualizarTarefaRequest
        {
            Descricao = tarefa.Descricao,
            Situacao = "Concluída"
        };

        var instanteAnterior = DateTime.UtcNow;

        // Act
        var resultado = await tarefaService.AtualizarAsync(
            tarefa.Id,
            request
        );

        var instantePosterior = DateTime.UtcNow;

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Concluída", resultado.Situacao);

        Assert.NotNull(resultado.ModificadaEm);
        Assert.NotNull(resultado.ConcluidaEm);

        Assert.Equal(InstanteControlado.UtcDateTime, resultado.ModificadaEm);

        Assert.Equal(
            resultado.ModificadaEm,
            resultado.SituacaoAlteradaEm
        );

        Assert.Equal(
            resultado.ModificadaEm,
            resultado.ConcluidaEm
        );

        Assert.Equal(
            1,
            tarefaRepository.QuantidadeChamadasSalvarAlteracoes
        );
    }

    [Fact]
    public async Task AtualizarAsync_ReabrindoTarefa_DeveLimparConcluidaEm()
    {
        // Arrange
        var criadaEm = DateTime.UtcNow.AddDays(-5);
        var conclusaoAnterior = DateTime.UtcNow.AddDays(-1);

        var tarefa = new Tarefa
        {
            Id = 50,
            Descricao = "Tarefa concluída",
            Situacao = "Concluída",
            CriadaEm = criadaEm,
            ModificadaEm = conclusaoAnterior,
            SituacaoAlteradaEm = conclusaoAnterior,
            ConcluidaEm = conclusaoAnterior,
            ExcluidaEm = null
        };

        var tarefaRepository = new TarefaRepositoryFake
        {
            TarefaRetornadaPorBuscaAtiva = tarefa
        };

        var tarefaService = CriarService(tarefaRepository);

        var request = new AtualizarTarefaRequest
        {
            Descricao = tarefa.Descricao,
            Situacao = "Em andamento"
        };

        var instanteAnterior = DateTime.UtcNow;

        // Act
        var resultado = await tarefaService.AtualizarAsync(
            tarefa.Id,
            request
        );

        var instantePosterior = DateTime.UtcNow;

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Em andamento", resultado.Situacao);
        Assert.Null(resultado.ConcluidaEm);
        Assert.NotNull(resultado.ModificadaEm);

        Assert.Equal(InstanteControlado.UtcDateTime, resultado.ModificadaEm);

        Assert.Equal(
            resultado.ModificadaEm,
            resultado.SituacaoAlteradaEm
        );

        Assert.Equal(
            1,
            tarefaRepository.QuantidadeChamadasSalvarAlteracoes
        );
    }

    [Fact]
    public async Task ExcluirLogicamenteAsync_TarefaExistente_DevePreencherExcluidaEmESalvar()
    {
        // Arrange
        var tarefa = CriarTarefa(
            id: 60,
            descricao: "Tarefa para excluir",
            situacao: "Pendente"
        );

        var tarefaRepository = new TarefaRepositoryFake
        {
            TarefaRetornadaPorBuscaAtiva = tarefa
        };

        var tarefaService = CriarService(tarefaRepository);

        var instanteAnterior = DateTime.UtcNow;

        // Act
        var resultado = await tarefaService.ExcluirLogicamenteAsync(
            tarefa.Id
        );

        var instantePosterior = DateTime.UtcNow;

        // Assert
        Assert.True(resultado);
        Assert.NotNull(tarefa.ExcluidaEm);

        Assert.Equal(InstanteControlado.UtcDateTime, tarefa.ExcluidaEm);

        Assert.Equal(
            tarefa.Id,
            tarefaRepository.UltimoIdBuscadoAtiva
        );

        Assert.True(
            tarefaRepository.UltimaBuscaAtivaRastreouAlteracoes
        );

        Assert.Equal(
            1,
            tarefaRepository.QuantidadeChamadasSalvarAlteracoes
        );
    }

    [Fact]
    public async Task ExcluirLogicamenteAsync_TarefaInexistente_DeveRetornarFalseENaoSalvar()
    {
        // Arrange
        var tarefaRepository = new TarefaRepositoryFake
        {
            TarefaRetornadaPorBuscaAtiva = null
        };

        var tarefaService = CriarService(tarefaRepository);

        const int tarefaId = 999;

        // Act
        var resultado = await tarefaService.ExcluirLogicamenteAsync(
            tarefaId
        );

        // Assert
        Assert.False(resultado);
        Assert.Equal(tarefaId, tarefaRepository.UltimoIdBuscadoAtiva);
        Assert.True(tarefaRepository.UltimaBuscaAtivaRastreouAlteracoes);

        Assert.Equal(
            0,
            tarefaRepository.QuantidadeChamadasSalvarAlteracoes
        );
    }

    [Fact]
    public async Task ExcluirPermanentementeAsync_TarefaInexistente_DeveRetornarNaoEncontrada()
    {
        // Arrange
        var tarefaRepository = new TarefaRepositoryFake
        {
            TarefaRetornadaPorBuscaIncluindoExcluidas = null
        };

        var tarefaService = CriarService(tarefaRepository);

        const int tarefaId = 999;

        // Act
        var resultado =
            await tarefaService.ExcluirPermanentementeAsync(
                tarefaId
            );

        // Assert
        Assert.Equal(
            ResultadoExclusaoPermanente.NaoEncontrada,
            resultado
        );

        Assert.Equal(
            tarefaId,
            tarefaRepository.UltimoIdBuscadoIncluindoExcluidas
        );

        Assert.Equal(
            0,
            tarefaRepository.QuantidadeChamadasRemover
        );

        Assert.Equal(
            0,
            tarefaRepository.QuantidadeChamadasSalvarAlteracoes
        );
    }

    [Fact]
    public async Task ExcluirPermanentementeAsync_TarefaAtiva_DeveBloquearExclusao()
    {
        // Arrange
        var tarefa = CriarTarefa(
            id: 70,
            descricao: "Tarefa ativa",
            situacao: "Pendente"
        );

        tarefa.ExcluidaEm = null;

        var tarefaRepository = new TarefaRepositoryFake
        {
            TarefaRetornadaPorBuscaIncluindoExcluidas = tarefa
        };

        var tarefaService = CriarService(tarefaRepository);

        // Act
        var resultado =
            await tarefaService.ExcluirPermanentementeAsync(
                tarefa.Id
            );

        // Assert
        Assert.Equal(
            ResultadoExclusaoPermanente.TarefaAtiva,
            resultado
        );

        Assert.Equal(
            0,
            tarefaRepository.QuantidadeChamadasRemover
        );

        Assert.Equal(
            0,
            tarefaRepository.QuantidadeChamadasSalvarAlteracoes
        );
    }

    [Fact]
    public async Task ExcluirPermanentementeAsync_TarefaExcluida_DeveRemoverESalvar()
    {
        // Arrange
        var tarefa = CriarTarefa(
            id: 80,
            descricao: "Tarefa na lixeira",
            situacao: "Concluída"
        );

        tarefa.ExcluidaEm = DateTime.UtcNow.AddHours(-1);

        var tarefaRepository = new TarefaRepositoryFake
        {
            TarefaRetornadaPorBuscaIncluindoExcluidas = tarefa
        };

        var tarefaService = CriarService(tarefaRepository);

        // Act
        var resultado =
            await tarefaService.ExcluirPermanentementeAsync(
                tarefa.Id
            );

        // Assert
        Assert.Equal(
            ResultadoExclusaoPermanente.Sucesso,
            resultado
        );

        Assert.Same(
            tarefa,
            tarefaRepository.TarefaRemovida
        );

        Assert.Equal(
            1,
            tarefaRepository.QuantidadeChamadasRemover
        );

        Assert.Equal(
            1,
            tarefaRepository.QuantidadeChamadasSalvarAlteracoes
        );
    }

    [Fact]
    public async Task ListarExcluidasAsync_DeveRetornarSomenteTarefasExcluidas()
    {
        // Arrange
        var tarefaExcluida = CriarTarefa(90, "Tarefa na lixeira", "Pendente");
        tarefaExcluida.ExcluidaEm = DateTime.UtcNow.AddHours(-1);

        var tarefaRepository = new TarefaRepositoryFake();
        tarefaRepository.TarefasExcluidas.Add(tarefaExcluida);

        var tarefaService = CriarService(tarefaRepository);

        // Act
        var resultado = await tarefaService.ListarExcluidasAsync();

        // Assert
        var tarefa = Assert.Single(resultado);
        Assert.Equal(tarefaExcluida.Id, tarefa.Id);
        Assert.NotNull(tarefa.ExcluidaEm);
        Assert.Equal(1, tarefaRepository.QuantidadeChamadasListarExcluidas);
        Assert.Equal(0, tarefaRepository.QuantidadeChamadasListarAtivas);
    }

    [Fact]
    public async Task RestaurarAsync_TarefaExcluida_DeveLimparExcluidaEmEPreservarEstado()
    {
        // Arrange
        var conclusao = DateTime.UtcNow.AddDays(-2);
        var exclusao = DateTime.UtcNow.AddHours(-1);
        var tarefa = CriarTarefa(100, "Tarefa concluída", "Concluída");
        tarefa.SituacaoAlteradaEm = conclusao;
        tarefa.ConcluidaEm = conclusao;
        tarefa.ExcluidaEm = exclusao;

        var tarefaRepository = new TarefaRepositoryFake
        {
            TarefaRetornadaPorBuscaIncluindoExcluidas = tarefa
        };
        var tarefaService = CriarService(tarefaRepository);
        var instanteAnterior = DateTime.UtcNow;

        // Act
        var resultado = await tarefaService.RestaurarAsync(tarefa.Id);

        var instantePosterior = DateTime.UtcNow;

        // Assert
        Assert.Equal(ResultadoRestauracao.Sucesso, resultado);
        Assert.Null(tarefa.ExcluidaEm);
        Assert.Equal("Concluída", tarefa.Situacao);
        Assert.Equal(conclusao, tarefa.SituacaoAlteradaEm);
        Assert.Equal(conclusao, tarefa.ConcluidaEm);
        Assert.NotNull(tarefa.ModificadaEm);
        Assert.Equal(InstanteControlado.UtcDateTime, tarefa.ModificadaEm);
        Assert.Equal(1, tarefaRepository.QuantidadeChamadasSalvarAlteracoes);
    }

    [Fact]
    public async Task RestaurarAsync_TarefaInexistente_DeveRetornarNaoEncontradaENaoSalvar()
    {
        // Arrange
        var tarefaRepository = new TarefaRepositoryFake
        {
            TarefaRetornadaPorBuscaIncluindoExcluidas = null
        };
        var tarefaService = CriarService(tarefaRepository);

        // Act
        var resultado = await tarefaService.RestaurarAsync(999);

        // Assert
        Assert.Equal(ResultadoRestauracao.NaoEncontrada, resultado);
        Assert.Equal(0, tarefaRepository.QuantidadeChamadasSalvarAlteracoes);
    }

    [Fact]
    public async Task RestaurarAsync_TarefaAtiva_DeveBloquearRestauracaoENaoSalvar()
    {
        // Arrange
        var tarefa = CriarTarefa(110, "Tarefa ativa", "Pendente");
        var tarefaRepository = new TarefaRepositoryFake
        {
            TarefaRetornadaPorBuscaIncluindoExcluidas = tarefa
        };
        var tarefaService = CriarService(tarefaRepository);

        // Act
        var resultado = await tarefaService.RestaurarAsync(tarefa.Id);

        // Assert
        Assert.Equal(ResultadoRestauracao.TarefaAtiva, resultado);
        Assert.Equal(0, tarefaRepository.QuantidadeChamadasSalvarAlteracoes);
    }

    [Fact]
    public async Task ListarAsync_ConsultaPadrao_DeveEnviarDefaultsAoRepositorio()
    {
        var repositorio = new TarefaRepositoryFake();

        await CriarService(repositorio).ListarAsync(new ConsultaTarefasRequest());

        var consulta = Assert.IsType<ConsultaTarefas>(repositorio.UltimaConsultaTarefas);
        Assert.Equal(1, consulta.Pagina);
        Assert.Equal(10, consulta.TamanhoPagina);
        Assert.Equal(CampoOrdenacaoTarefa.UltimaAtualizacao, consulta.OrdenarPor);
        Assert.Equal(DirecaoOrdenacao.Desc, consulta.Direcao);
        Assert.Null(consulta.Busca);
        Assert.Null(consulta.Situacao);
    }

    [Fact]
    public async Task ListarAsync_ConsultaValida_DeveNormalizarEMapearResultadoPaginado()
    {
        var tarefa = CriarTarefa(1, "Relatório", "Pendente");
        var repositorio = new TarefaRepositoryFake
        {
            ResultadoConsultaConfigurado = new ResultadoConsultaTarefas
            {
                Itens = [tarefa],
                TotalItens = 21
            }
        };

        var resultado = await CriarService(repositorio).ListarAsync(new ConsultaTarefasRequest
        {
            Busca = "  relatório  ", Situacao = " pendente ", OrdenarPor = "descricao",
            Direcao = "asc", Pagina = 2, TamanhoPagina = 10
        });

        Assert.Equal("relatório", repositorio.UltimaConsultaTarefas!.Busca);
        Assert.Equal("Pendente", repositorio.UltimaConsultaTarefas.Situacao);
        Assert.Equal(CampoOrdenacaoTarefa.Descricao, repositorio.UltimaConsultaTarefas.OrdenarPor);
        Assert.Equal(DirecaoOrdenacao.Asc, repositorio.UltimaConsultaTarefas.Direcao);
        Assert.Equal(2, resultado.PaginaAtual);
        Assert.Equal(10, resultado.TamanhoPagina);
        Assert.Equal(21, resultado.TotalItens);
        Assert.Equal(3, resultado.TotalPaginas);
        Assert.Equal(tarefa.Id, Assert.Single(resultado.Itens).Id);
    }

    [Theory]
    [InlineData("Cancelada")]
    public async Task ListarAsync_SituacaoInvalida_DeveRejeitar(string situacao)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => CriarService(new TarefaRepositoryFake())
            .ListarAsync(new ConsultaTarefasRequest { Situacao = situacao }));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ListarAsync_PaginaInvalida_DeveRejeitar(int pagina)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => CriarService(new TarefaRepositoryFake())
            .ListarAsync(new ConsultaTarefasRequest { Pagina = pagina }));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task ListarAsync_TamanhoPaginaInvalido_DeveRejeitar(int tamanhoPagina)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => CriarService(new TarefaRepositoryFake())
            .ListarAsync(new ConsultaTarefasRequest { TamanhoPagina = tamanhoPagina }));
    }

    [Theory]
    [InlineData("id")]
    [InlineData("crescente")]
    public async Task ListarAsync_OrdenacaoOuDirecaoInvalida_DeveRejeitar(string valor)
    {
        var service = CriarService(new TarefaRepositoryFake());
        await Assert.ThrowsAsync<ArgumentException>(() => service.ListarAsync(new ConsultaTarefasRequest { OrdenarPor = valor }));
        await Assert.ThrowsAsync<ArgumentException>(() => service.ListarAsync(new ConsultaTarefasRequest { Direcao = valor }));
    }

    [Fact]
    public async Task CriarAsync_DeveRegistrarHistoricoDeCriacaoNoMesmoSave()
    {
        var repositorio = new TarefaRepositoryFake();
        var cancellationToken = new CancellationTokenSource().Token;

        await CriarService(repositorio).CriarAsync(new CriarTarefaRequest
        {
            Descricao = "Nova tarefa"
        }, cancellationToken);

        var historico = Assert.IsType<HistoricoTarefa>(repositorio.HistoricoAdicionado);
        Assert.Equal(TiposHistoricoTarefa.Criacao, historico.Tipo);
        Assert.Equal(InstanteControlado.UtcDateTime, historico.CriadoEm);
        Assert.Same(repositorio.TarefaAdicionada, historico.Tarefa);
        Assert.Equal(1, repositorio.QuantidadeChamadasSalvarAlteracoes);
        Assert.Equal(cancellationToken, repositorio.UltimoCancellationToken);
    }

    [Fact]
    public async Task AtualizarAsync_AlterandoDescricao_DeveRegistrarValoresAnteriorENovo()
    {
        var tarefa = CriarTarefa(120, "DescriÃ§Ã£o anterior", "Pendente");
        var repositorio = new TarefaRepositoryFake { TarefaRetornadaPorBuscaAtiva = tarefa };

        await CriarService(repositorio).AtualizarAsync(tarefa.Id, new AtualizarTarefaRequest
        {
            Descricao = "DescriÃ§Ã£o nova",
            Situacao = tarefa.Situacao
        });

        var historico = Assert.IsType<HistoricoTarefa>(repositorio.HistoricoAdicionado);
        Assert.Equal(TiposHistoricoTarefa.AlteracaoDescricao, historico.Tipo);
        Assert.Equal("Descricao", historico.Campo);
        Assert.Equal("DescriÃ§Ã£o anterior", historico.ValorAnterior);
        Assert.Equal("DescriÃ§Ã£o nova", historico.ValorNovo);
        Assert.Equal(InstanteControlado.UtcDateTime, historico.CriadoEm);
    }

    [Fact]
    public async Task AtualizarAsync_ConcluindoTarefa_DeveRegistrarHistoricoDeConclusao()
    {
        var tarefa = CriarTarefa(121, "Concluir", "Em andamento");
        var repositorio = new TarefaRepositoryFake { TarefaRetornadaPorBuscaAtiva = tarefa };

        await CriarService(repositorio).AtualizarAsync(tarefa.Id, new AtualizarTarefaRequest
        {
            Descricao = tarefa.Descricao,
            Situacao = SituacoesTarefa.Concluida
        });

        var historico = Assert.IsType<HistoricoTarefa>(repositorio.HistoricoAdicionado);
        Assert.Equal(TiposHistoricoTarefa.Conclusao, historico.Tipo);
        Assert.Equal("Situacao", historico.Campo);
        Assert.Equal(SituacoesTarefa.EmAndamento, historico.ValorAnterior);
        Assert.Equal(SituacoesTarefa.Concluida, historico.ValorNovo);
    }

    [Fact]
    public async Task AtualizarAsync_ReabrindoTarefa_DeveRegistrarHistoricoDeReabertura()
    {
        var tarefa = CriarTarefa(122, "Reabrir", SituacoesTarefa.Concluida);
        var repositorio = new TarefaRepositoryFake { TarefaRetornadaPorBuscaAtiva = tarefa };

        await CriarService(repositorio).AtualizarAsync(tarefa.Id, new AtualizarTarefaRequest
        {
            Descricao = tarefa.Descricao,
            Situacao = "Pendente"
        });

        var historico = Assert.IsType<HistoricoTarefa>(repositorio.HistoricoAdicionado);
        Assert.Equal(TiposHistoricoTarefa.Reabertura, historico.Tipo);
        Assert.Equal("Situacao", historico.Campo);
        Assert.Equal(SituacoesTarefa.Concluida, historico.ValorAnterior);
        Assert.Equal(SituacoesTarefa.Pendente, historico.ValorNovo);
    }

    [Theory]
    [InlineData(SituacoesTarefa.Pendente, SituacoesTarefa.EmAndamento, TiposHistoricoTarefa.AlteracaoSituacao, false)]
    [InlineData(SituacoesTarefa.Pendente, SituacoesTarefa.Concluida, TiposHistoricoTarefa.Conclusao, true)]
    [InlineData(SituacoesTarefa.EmAndamento, SituacoesTarefa.Pendente, TiposHistoricoTarefa.AlteracaoSituacao, false)]
    [InlineData(SituacoesTarefa.EmAndamento, SituacoesTarefa.Concluida, TiposHistoricoTarefa.Conclusao, true)]
    [InlineData(SituacoesTarefa.Concluida, SituacoesTarefa.Pendente, TiposHistoricoTarefa.Reabertura, false)]
    [InlineData(SituacoesTarefa.Concluida, SituacoesTarefa.EmAndamento, TiposHistoricoTarefa.Reabertura, false)]
    public async Task AtualizarAsync_TransicoesPermitidas_DevemSerAuditadas(
        string origem,
        string destino,
        string tipoHistorico,
        bool deveConcluir)
    {
        var tarefa = CriarTarefa(130, "Transição", origem);
        var repositorio = new TarefaRepositoryFake { TarefaRetornadaPorBuscaAtiva = tarefa };

        var resultado = await CriarService(repositorio).AtualizarAsync(tarefa.Id, new AtualizarTarefaRequest
        {
            Descricao = tarefa.Descricao,
            Situacao = destino
        });

        var historico = Assert.Single(repositorio.HistoricoTarefas);
        Assert.NotNull(resultado);
        Assert.Equal(destino, resultado.Situacao);
        Assert.Equal(tipoHistorico, historico.Tipo);
        Assert.Equal("Situacao", historico.Campo);
        Assert.Equal(origem, historico.ValorAnterior);
        Assert.Equal(destino, historico.ValorNovo);
        Assert.Equal(InstanteControlado.UtcDateTime, historico.CriadoEm);
        Assert.Equal(InstanteControlado.UtcDateTime, resultado.SituacaoAlteradaEm);
        Assert.Equal(deveConcluir ? InstanteControlado.UtcDateTime : null, resultado.ConcluidaEm);
        Assert.Equal(1, repositorio.QuantidadeChamadasSalvarAlteracoes);
    }

    [Theory]
    [InlineData(SituacoesTarefa.Pendente)]
    [InlineData(SituacoesTarefa.EmAndamento)]
    [InlineData(SituacoesTarefa.Concluida)]
    public async Task AtualizarAsync_MesmaSituacao_NaoDeveGerarHistoricoNemAlterarDatas(string situacao)
    {
        var tarefa = CriarTarefa(131, "Sem transição", situacao);
        var modificadaEm = tarefa.ModificadaEm;
        var situacaoAlteradaEm = tarefa.SituacaoAlteradaEm;
        var concluidaEm = tarefa.ConcluidaEm;
        var repositorio = new TarefaRepositoryFake { TarefaRetornadaPorBuscaAtiva = tarefa };

        await CriarService(repositorio).AtualizarAsync(tarefa.Id, new AtualizarTarefaRequest
        {
            Descricao = tarefa.Descricao,
            Situacao = situacao
        });

        Assert.Empty(repositorio.HistoricoTarefas);
        Assert.Equal(modificadaEm, tarefa.ModificadaEm);
        Assert.Equal(situacaoAlteradaEm, tarefa.SituacaoAlteradaEm);
        Assert.Equal(concluidaEm, tarefa.ConcluidaEm);
        Assert.Equal(0, repositorio.QuantidadeChamadasSalvarAlteracoes);
    }

    [Fact]
    public async Task AtualizarAsync_ReentrandoEmConcluida_DevePreservarHistoricoEDefinirNovaData()
    {
        var tarefa = CriarTarefa(132, "Novo ciclo", SituacoesTarefa.Pendente);
        var repositorio = new TarefaRepositoryFake { TarefaRetornadaPorBuscaAtiva = tarefa };
        var primeiraConclusao = InstanteControlado.AddHours(1);
        var reabertura = primeiraConclusao.AddHours(1);
        var segundaConclusao = reabertura.AddHours(1);

        await CriarService(repositorio, new TimeProviderFixo(primeiraConclusao)).AtualizarAsync(tarefa.Id, new AtualizarTarefaRequest { Descricao = tarefa.Descricao, Situacao = SituacoesTarefa.Concluida });
        await CriarService(repositorio, new TimeProviderFixo(reabertura)).AtualizarAsync(tarefa.Id, new AtualizarTarefaRequest { Descricao = tarefa.Descricao, Situacao = SituacoesTarefa.Pendente });
        var resultado = await CriarService(repositorio, new TimeProviderFixo(segundaConclusao)).AtualizarAsync(tarefa.Id, new AtualizarTarefaRequest { Descricao = tarefa.Descricao, Situacao = SituacoesTarefa.Concluida });

        Assert.NotNull(resultado);
        Assert.Equal(segundaConclusao.UtcDateTime, resultado.ConcluidaEm);
        Assert.Equal(2, repositorio.HistoricoTarefas.Count(item => item.Tipo == TiposHistoricoTarefa.Conclusao));
        Assert.Collection(repositorio.HistoricoTarefas,
            item => Assert.Equal(primeiraConclusao.UtcDateTime, item.CriadoEm),
            item => Assert.Equal(TiposHistoricoTarefa.Reabertura, item.Tipo),
            item => Assert.Equal(segundaConclusao.UtcDateTime, item.CriadoEm));
    }

    [Fact]
    public async Task AtualizarAsync_AlteracoesMultiplas_DevemRegistrarCadaHistoricoEmUmUnicoSave()
    {
        var tarefa = CriarTarefa(133, "Antes", SituacoesTarefa.Pendente);
        var repositorio = new TarefaRepositoryFake { TarefaRetornadaPorBuscaAtiva = tarefa };

        await CriarService(repositorio).AtualizarAsync(tarefa.Id, new AtualizarTarefaRequest
        {
            Descricao = "Depois",
            Situacao = SituacoesTarefa.EmAndamento,
            Prioridade = PrioridadesTarefa.Alta
        });

        Assert.Equal(3, repositorio.HistoricoTarefas.Count);
        Assert.Contains(repositorio.HistoricoTarefas, item => item.Tipo == TiposHistoricoTarefa.AlteracaoDescricao);
        Assert.Contains(repositorio.HistoricoTarefas, item => item.Tipo == TiposHistoricoTarefa.AlteracaoPrioridade);
        Assert.Contains(repositorio.HistoricoTarefas, item => item.Tipo == TiposHistoricoTarefa.AlteracaoSituacao && item.ValorAnterior == SituacoesTarefa.Pendente && item.ValorNovo == SituacoesTarefa.EmAndamento);
        Assert.Equal(1, repositorio.QuantidadeChamadasSalvarAlteracoes);
    }

    [Fact]
    public async Task ExcluirLogicamenteAsync_DeveRegistrarHistoricoDeExclusao()
    {
        var tarefa = CriarTarefa(123, "Excluir", "Pendente");
        var repositorio = new TarefaRepositoryFake { TarefaRetornadaPorBuscaAtiva = tarefa };

        await CriarService(repositorio).ExcluirLogicamenteAsync(tarefa.Id);

        Assert.Equal(TiposHistoricoTarefa.Exclusao, repositorio.HistoricoAdicionado?.Tipo);
        Assert.Equal(InstanteControlado.UtcDateTime, repositorio.HistoricoAdicionado?.CriadoEm);
    }

    [Fact]
    public async Task RestaurarAsync_DeveRegistrarHistoricoDeRestauracao()
    {
        var tarefa = CriarTarefa(124, "Restaurar", "Pendente");
        tarefa.ExcluidaEm = InstanteControlado.UtcDateTime.AddDays(-1);
        var repositorio = new TarefaRepositoryFake { TarefaRetornadaPorBuscaIncluindoExcluidas = tarefa };

        await CriarService(repositorio).RestaurarAsync(tarefa.Id);

        Assert.Equal(TiposHistoricoTarefa.Restauracao, repositorio.HistoricoAdicionado?.Tipo);
    }

    [Fact]
    public async Task AtualizarAsync_SemAlteracoes_NaoDeveRegistrarHistorico()
    {
        var tarefa = CriarTarefa(125, "Sem alteraÃ§Ã£o", "Pendente");
        var repositorio = new TarefaRepositoryFake { TarefaRetornadaPorBuscaAtiva = tarefa };

        await CriarService(repositorio).AtualizarAsync(tarefa.Id, new AtualizarTarefaRequest
        {
            Descricao = tarefa.Descricao,
            Situacao = tarefa.Situacao
        });

        Assert.Null(repositorio.HistoricoAdicionado);
        Assert.Equal(0, repositorio.QuantidadeChamadasSalvarAlteracoes);
    }

    [Fact]
    public async Task ListarHistoricoAsync_TarefaExcluida_DeveRetornarHistoricoNaOrdemDoRepositorio()
    {
        var tarefa = CriarTarefa(126, "Na lixeira", "Pendente");
        tarefa.ExcluidaEm = InstanteControlado.UtcDateTime;
        var maisRecente = new HistoricoTarefa { Id = 2, TarefaId = tarefa.Id, Tipo = TiposHistoricoTarefa.Exclusao, CriadoEm = InstanteControlado.UtcDateTime };
        var maisAntigo = new HistoricoTarefa { Id = 1, TarefaId = tarefa.Id, Tipo = TiposHistoricoTarefa.Criacao, CriadoEm = InstanteControlado.UtcDateTime.AddDays(-1) };
        var repositorio = new TarefaRepositoryFake { TarefaRetornadaPorBuscaIncluindoExcluidas = tarefa };
        repositorio.HistoricoTarefas.AddRange([maisRecente, maisAntigo]);

        var resultado = await CriarService(repositorio).ListarHistoricoAsync(tarefa.Id);

        Assert.NotNull(resultado);
        Assert.Collection(resultado,
            item => Assert.Equal(TiposHistoricoTarefa.Exclusao, item.Tipo),
            item => Assert.Equal(TiposHistoricoTarefa.Criacao, item.Tipo));
        Assert.Equal(tarefa.Id, repositorio.UltimoIdHistoricoConsultado);
    }

    [Fact]
    public async Task ListarHistoricoAsync_TarefaInexistente_NaoDeveConsultarHistorico()
    {
        var repositorio = new TarefaRepositoryFake();

        var resultado = await CriarService(repositorio).ListarHistoricoAsync(999);

        Assert.Null(resultado);
        Assert.Equal(0, repositorio.QuantidadeChamadasListarHistorico);
    }

    [Fact]
    public async Task CriarAsync_SemPrioridade_DeveUsarMediaEManterVencimentoNulo()
    {
        var repositorio = new TarefaRepositoryFake();

        var resultado = await CriarService(repositorio).CriarAsync(new CriarTarefaRequest { Descricao = "Tarefa padr\u00e3o" });

        Assert.Equal(PrioridadesTarefa.Media, resultado.Prioridade);
        Assert.Null(resultado.DataVencimento);
    }

    [Fact]
    public async Task AtualizarAsync_AlterandoPrioridadeEVencimento_DeveRegistrarHistoricosCorretos()
    {
        var tarefa = CriarTarefa(127, "Planejar", SituacoesTarefa.Pendente);
        tarefa.DataVencimento = new DateOnly(2030, 1, 5);
        var repositorio = new TarefaRepositoryFake { TarefaRetornadaPorBuscaAtiva = tarefa };

        var resultado = await CriarService(repositorio).AtualizarAsync(tarefa.Id, new AtualizarTarefaRequest
        {
            Descricao = tarefa.Descricao,
            Situacao = tarefa.Situacao,
            Prioridade = PrioridadesTarefa.Alta,
            DataVencimento = new DateOnly(2030, 1, 7)
        });

        Assert.NotNull(resultado);
        Assert.Equal(PrioridadesTarefa.Alta, resultado.Prioridade);
        Assert.Equal(new DateOnly(2030, 1, 7), resultado.DataVencimento);
        Assert.Collection(repositorio.HistoricoTarefas,
            item =>
            {
                Assert.Equal(TiposHistoricoTarefa.AlteracaoPrioridade, item.Tipo);
                Assert.Equal(PrioridadesTarefa.Media, item.ValorAnterior);
                Assert.Equal(PrioridadesTarefa.Alta, item.ValorNovo);
            },
            item =>
            {
                Assert.Equal(TiposHistoricoTarefa.AlteracaoDataVencimento, item.Tipo);
                Assert.Equal("2030-01-05", item.ValorAnterior);
                Assert.Equal("2030-01-07", item.ValorNovo);
            });
    }

    [Fact]
    public async Task AtualizarAsync_RemovendoVencimento_DeveRegistrarNovoValorNulo()
    {
        var tarefa = CriarTarefa(128, "Remover prazo", SituacoesTarefa.Pendente);
        tarefa.DataVencimento = new DateOnly(2030, 1, 5);
        var repositorio = new TarefaRepositoryFake { TarefaRetornadaPorBuscaAtiva = tarefa };

        await CriarService(repositorio).AtualizarAsync(tarefa.Id, new AtualizarTarefaRequest
        {
            Descricao = tarefa.Descricao,
            Situacao = tarefa.Situacao,
            Prioridade = tarefa.Prioridade,
            DataVencimento = null
        });

        var historico = Assert.Single(repositorio.HistoricoTarefas);
        Assert.Equal(TiposHistoricoTarefa.AlteracaoDataVencimento, historico.Tipo);
        Assert.Equal("2030-01-05", historico.ValorAnterior);
        Assert.Null(historico.ValorNovo);
    }

    [Fact]
    public async Task AtualizarAsync_SemMudancasNosNovosCampos_NaoDeveSalvar()
    {
        var tarefa = CriarTarefa(129, "Sem mudan\u00e7as", SituacoesTarefa.Pendente);
        tarefa.DataVencimento = new DateOnly(2030, 1, 5);
        var repositorio = new TarefaRepositoryFake { TarefaRetornadaPorBuscaAtiva = tarefa };

        await CriarService(repositorio).AtualizarAsync(tarefa.Id, new AtualizarTarefaRequest
        {
            Descricao = tarefa.Descricao,
            Situacao = tarefa.Situacao,
            Prioridade = tarefa.Prioridade,
            DataVencimento = tarefa.DataVencimento
        });

        Assert.Empty(repositorio.HistoricoTarefas);
        Assert.Equal(0, repositorio.QuantidadeChamadasSalvarAlteracoes);
    }

    [Fact]
    public async Task ListarAsync_FiltrosNovos_DeveNormalizarEUsarDataDoTimeProvider()
    {
        var repositorio = new TarefaRepositoryFake();

        await CriarService(repositorio).ListarAsync(new ConsultaTarefasRequest
        {
            Prioridade = " alta ",
            Prazo = "vencidas",
            OrdenarPor = "dataVencimento"
        });

        var consulta = Assert.IsType<ConsultaTarefas>(repositorio.UltimaConsultaTarefas);
        Assert.Equal(PrioridadesTarefa.Alta, consulta.Prioridade);
        Assert.Equal(FiltroPrazoTarefa.Vencidas, consulta.Prazo);
        Assert.Equal(CampoOrdenacaoTarefa.DataVencimento, consulta.OrdenarPor);
        Assert.Equal(new DateOnly(2030, 1, 2), consulta.Hoje);
    }

    [Theory]
    [InlineData("urgente")]
    public async Task ListarAsync_PrioridadeInvalida_DeveRejeitar(string prioridade)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => CriarService(new TarefaRepositoryFake())
            .ListarAsync(new ConsultaTarefasRequest { Prioridade = prioridade }));
    }

    [Fact]
    public async Task CriarAsync_ObservacoesAusentesOuComWhitespace_DevePersistirNulo()
    {
        var repositorio = new TarefaRepositoryFake();

        var semObservacoes = await CriarService(repositorio).CriarAsync(new CriarTarefaRequest { Descricao = "Sem observações" });
        var somenteEspacos = await CriarService(repositorio).CriarAsync(new CriarTarefaRequest { Descricao = "Espaços", Observacoes = "   " });

        Assert.Null(semObservacoes.Observacoes);
        Assert.Null(somenteEspacos.Observacoes);
    }

    [Fact]
    public async Task CriarAsync_ObservacoesComEspacos_DeveNormalizar()
    {
        var resultado = await CriarService(new TarefaRepositoryFake()).CriarAsync(new CriarTarefaRequest
        {
            Descricao = "Com observações",
            Observacoes = "  Primeiro item\nSegundo item  "
        });

        Assert.Equal("Primeiro item\nSegundo item", resultado.Observacoes);
    }

    [Fact]
    public void CriarTarefaRequest_ObservacoesAcimaDoLimite_DeveSerInvalido()
    {
        var request = new CriarTarefaRequest { Descricao = "Limite", Observacoes = new string('a', 4001) };
        var resultados = new List<ValidationResult>();

        var valido = Validator.TryValidateObject(request, new ValidationContext(request), resultados, validateAllProperties: true);

        Assert.False(valido);
        Assert.Contains(resultados, resultado => resultado.ErrorMessage == "As observações da tarefa devem ter no máximo 4000 caracteres.");
    }

    [Theory]
    [InlineData(null, "Nova observação", null, "Nova observação")]
    [InlineData("Anterior", "Outra observação", "Anterior", "Outra observação")]
    [InlineData("Anterior", "   ", "Anterior", null)]
    public async Task AtualizarAsync_AlterandoObservacoes_DeveRegistrarHistorico(
        string? observacoesAtuais,
        string? novasObservacoes,
        string? valorAnteriorEsperado,
        string? valorNovoEsperado)
    {
        var tarefa = CriarTarefa(130, "Observações", SituacoesTarefa.Pendente);
        tarefa.Observacoes = observacoesAtuais;
        var repositorio = new TarefaRepositoryFake { TarefaRetornadaPorBuscaAtiva = tarefa };

        var resultado = await CriarService(repositorio).AtualizarAsync(tarefa.Id, new AtualizarTarefaRequest
        {
            Descricao = tarefa.Descricao,
            Situacao = tarefa.Situacao,
            Prioridade = tarefa.Prioridade,
            Observacoes = novasObservacoes
        });

        var historico = Assert.Single(repositorio.HistoricoTarefas);
        Assert.Equal(TiposHistoricoTarefa.AlteracaoObservacoes, historico.Tipo);
        Assert.Equal("Observacoes", historico.Campo);
        Assert.Equal(valorAnteriorEsperado, historico.ValorAnterior);
        Assert.Equal(valorNovoEsperado, historico.ValorNovo);
        Assert.Equal(valorNovoEsperado, resultado?.Observacoes);
        Assert.Equal(1, repositorio.QuantidadeChamadasSalvarAlteracoes);
    }

    [Fact]
    public async Task AtualizarAsync_ObservacoesNormalizadasIguais_NaoDeveSalvarNemGerarHistorico()
    {
        var tarefa = CriarTarefa(131, "Sem alteração", SituacoesTarefa.Pendente);
        tarefa.Observacoes = "Teste";
        var repositorio = new TarefaRepositoryFake { TarefaRetornadaPorBuscaAtiva = tarefa };

        await CriarService(repositorio).AtualizarAsync(tarefa.Id, new AtualizarTarefaRequest
        {
            Descricao = tarefa.Descricao,
            Situacao = tarefa.Situacao,
            Prioridade = tarefa.Prioridade,
            Observacoes = "  Teste  "
        });

        Assert.Empty(repositorio.HistoricoTarefas);
        Assert.Equal(0, repositorio.QuantidadeChamadasSalvarAlteracoes);
    }

    [Fact]
    public async Task AtualizarAsync_DescricaoPrioridadeEObservacoes_DeveRegistrarCadaHistoricoEmUmUnicoSave()
    {
        var tarefa = CriarTarefa(132, "Antes", SituacoesTarefa.Pendente);
        tarefa.Prioridade = PrioridadesTarefa.Baixa;
        var repositorio = new TarefaRepositoryFake { TarefaRetornadaPorBuscaAtiva = tarefa };

        await CriarService(repositorio).AtualizarAsync(tarefa.Id, new AtualizarTarefaRequest
        {
            Descricao = "Depois",
            Situacao = tarefa.Situacao,
            Prioridade = PrioridadesTarefa.Alta,
            Observacoes = "Detalhe"
        });

        Assert.Equal(3, repositorio.HistoricoTarefas.Count);
        Assert.Contains(repositorio.HistoricoTarefas, item => item.Tipo == TiposHistoricoTarefa.AlteracaoDescricao);
        Assert.Contains(repositorio.HistoricoTarefas, item => item.Tipo == TiposHistoricoTarefa.AlteracaoPrioridade);
        Assert.Contains(repositorio.HistoricoTarefas, item => item.Tipo == TiposHistoricoTarefa.AlteracaoObservacoes);
        Assert.Equal(1, repositorio.QuantidadeChamadasSalvarAlteracoes);
    }

    private static TarefaService CriarService(
        TarefaRepositoryFake tarefaRepository,
        TimeProvider? timeProvider = null)
    {
        return new TarefaService(
            tarefaRepository,
            NullLogger<TarefaService>.Instance,
            timeProvider ?? new TimeProviderFixo(InstanteControlado),
            usuarioAtual: new UsuarioAtualFake()
        );
    }

    private static Tarefa CriarTarefa(
        int id,
        string descricao,
        string situacao)
    {
        var criadaEm = DateTime.UtcNow.AddDays(-5);

        return new Tarefa
        {
            Id = id,
            Descricao = descricao,
            Situacao = situacao,
            CriadaEm = criadaEm,
            ModificadaEm = null,
            SituacaoAlteradaEm = criadaEm,
            ConcluidaEm = string.Equals(
                situacao,
                "Concluída",
                StringComparison.OrdinalIgnoreCase
            )
                ? criadaEm
                : null,
            ExcluidaEm = null
        };
    }
}

internal sealed class TimeProviderFixo(DateTimeOffset utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => utcNow;
}
