using Microsoft.Extensions.Logging.Abstractions;
using MinhaPrimeiraAPI.DTOs.Requests;
using MinhaPrimeiraAPI.Models;
using MinhaPrimeiraAPI.Services;
using MinhaPrimeiraAPI.Tests.Fakes;
using Xunit;

namespace MinhaPrimeiraAPI.Tests.Services;

public sealed class TarefaServiceTests
{
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
        var resultado = await tarefaService.ListarAsync();

        // Assert
        Assert.Equal(2, resultado.Count);

        Assert.Equal(primeiraTarefa.Id, resultado[0].Id);
        Assert.Equal(primeiraTarefa.Descricao, resultado[0].Descricao);
        Assert.Equal(primeiraTarefa.Situacao, resultado[0].Situacao);
        Assert.Equal(primeiraTarefa.CriadaEm, resultado[0].CriadaEm);
        Assert.Equal(primeiraTarefa.ModificadaEm, resultado[0].ModificadaEm);
        Assert.Equal(
            primeiraTarefa.SituacaoAlteradaEm,
            resultado[0].SituacaoAlteradaEm
        );
        Assert.Equal(
            primeiraTarefa.ConcluidaEm,
            resultado[0].ConcluidaEm
        );
        Assert.Equal(
            primeiraTarefa.ExcluidaEm,
            resultado[0].ExcluidaEm
        );

        Assert.Equal(segundaTarefa.Id, resultado[1].Id);
        Assert.Equal(segundaTarefa.Descricao, resultado[1].Descricao);
        Assert.Equal(segundaTarefa.Situacao, resultado[1].Situacao);

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

        Assert.InRange(
            resultado.CriadaEm,
            instanteAnterior,
            instantePosterior
        );

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

        Assert.InRange(
            resultado.ConcluidaEm.Value,
            instanteAnterior,
            instantePosterior
        );

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

        Assert.InRange(
            resultado.ModificadaEm.Value,
            instanteAnterior,
            instantePosterior
        );

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

        Assert.InRange(
            resultado.ModificadaEm.Value,
            instanteAnterior,
            instantePosterior
        );

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

        Assert.InRange(
            resultado.ModificadaEm.Value,
            instanteAnterior,
            instantePosterior
        );

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

        Assert.InRange(
            tarefa.ExcluidaEm.Value,
            instanteAnterior,
            instantePosterior
        );

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

    private static TarefaService CriarService(
        TarefaRepositoryFake tarefaRepository)
    {
        return new TarefaService(
            tarefaRepository,
            NullLogger<TarefaService>.Instance
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
