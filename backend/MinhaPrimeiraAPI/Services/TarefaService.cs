using MinhaPrimeiraAPI.DTOs.Requests;
using MinhaPrimeiraAPI.DTOs.Responses;
using MinhaPrimeiraAPI.Models;
using MinhaPrimeiraAPI.Repositories;

namespace MinhaPrimeiraAPI.Services;

public class TarefaService : ITarefaService
{
    private readonly ITarefaRepository _tarefaRepository;
    private readonly ILogger<TarefaService> _logger;

    public TarefaService(
        ITarefaRepository tarefaRepository,
        ILogger<TarefaService> logger)
    {
        _tarefaRepository = tarefaRepository;
        _logger = logger;
    }

    public async Task<List<TarefaResponse>> ListarAsync()
    {
        var tarefasEncontradas =
            await _tarefaRepository.ListarAtivasAsync();

        _logger.LogInformation(
            "Listagem de tarefas concluída. Quantidade={Quantidade}",
            tarefasEncontradas.Count
        );

        return tarefasEncontradas
            .Select(MapearParaResponse)
            .ToList();
    }

    public async Task<TarefaResponse?> BuscarPorIdAsync(int id)
    {
        var tarefaEncontrada =
            await _tarefaRepository.BuscarAtivaPorIdAsync(id);

        if (tarefaEncontrada is null)
        {
            _logger.LogWarning(
                "Tarefa não encontrada. TarefaId={TarefaId}",
                id
            );

            return null;
        }

        return MapearParaResponse(tarefaEncontrada);
    }

    public async Task<TarefaResponse> CriarAsync(
        CriarTarefaRequest novaTarefa)
    {
        var agora = DateTime.UtcNow;

        var situacao =
            NormalizarSituacaoCriacao(
                novaTarefa.Situacao
            );

        var tarefa = new Tarefa
        {
            Descricao = novaTarefa.Descricao.Trim(),
            Situacao = situacao,
            CriadaEm = agora,
            ModificadaEm = null,
            SituacaoAlteradaEm = agora,
            ConcluidaEm = EstaConcluida(situacao)
                ? agora
                : null,
            ExcluidaEm = null
        };

        _tarefaRepository.Adicionar(tarefa);
        await _tarefaRepository.SalvarAlteracoesAsync();

        _logger.LogInformation(
            "Tarefa criada. TarefaId={TarefaId}. CriadaEm={CriadaEm}",
            tarefa.Id,
            tarefa.CriadaEm
        );

        return MapearParaResponse(tarefa);
    }

    public async Task<TarefaResponse?> AtualizarAsync(
        int id,
        AtualizarTarefaRequest dadosAtualizados)
    {
        var tarefaEncontrada =
            await _tarefaRepository.BuscarAtivaPorIdAsync(
                id,
                rastrearAlteracoes: true
            );

        if (tarefaEncontrada is null)
        {
            _logger.LogWarning(
                "Tarefa não encontrada para atualização. TarefaId={TarefaId}",
                id
            );

            return null;
        }

        var novaDescricao =
            dadosAtualizados.Descricao.Trim();

        var novaSituacao =
            dadosAtualizados.Situacao.Trim();

        var descricaoAlterada =
            !string.Equals(
                tarefaEncontrada.Descricao,
                novaDescricao,
                StringComparison.Ordinal
            );

        var situacaoAlterada =
            !string.Equals(
                tarefaEncontrada.Situacao,
                novaSituacao,
                StringComparison.Ordinal
            );

        if (!descricaoAlterada && !situacaoAlterada)
        {
            _logger.LogInformation(
                "Atualização ignorada porque não houve alterações. TarefaId={TarefaId}",
                id
            );

            return MapearParaResponse(
                tarefaEncontrada
            );
        }

        var agora = DateTime.UtcNow;

        if (descricaoAlterada)
        {
            tarefaEncontrada.Descricao =
                novaDescricao;
        }

        if (situacaoAlterada)
        {
            tarefaEncontrada.Situacao =
                novaSituacao;

            tarefaEncontrada.SituacaoAlteradaEm =
                agora;

            tarefaEncontrada.ConcluidaEm =
                EstaConcluida(novaSituacao)
                    ? agora
                    : null;
        }

        tarefaEncontrada.ModificadaEm = agora;

        await _tarefaRepository.SalvarAlteracoesAsync();

        _logger.LogInformation(
            "Tarefa atualizada. TarefaId={TarefaId}. DescricaoAlterada={DescricaoAlterada}. SituacaoAlterada={SituacaoAlterada}. ModificadaEm={ModificadaEm}",
            id,
            descricaoAlterada,
            situacaoAlterada,
            tarefaEncontrada.ModificadaEm
        );

        return MapearParaResponse(
            tarefaEncontrada
        );
    }

    public async Task<bool> ExcluirLogicamenteAsync(int id)
    {
        var tarefaEncontrada =
            await _tarefaRepository.BuscarAtivaPorIdAsync(
                id,
                rastrearAlteracoes: true
            );

        if (tarefaEncontrada is null)
        {
            _logger.LogWarning(
                "Tarefa não encontrada para exclusão lógica. TarefaId={TarefaId}",
                id
            );

            return false;
        }

        tarefaEncontrada.ExcluidaEm =
            DateTime.UtcNow;

        await _tarefaRepository.SalvarAlteracoesAsync();

        _logger.LogInformation(
            "Tarefa excluída logicamente. TarefaId={TarefaId}. ExcluidaEm={ExcluidaEm}",
            tarefaEncontrada.Id,
            tarefaEncontrada.ExcluidaEm
        );

        return true;
    }

    public async Task<ResultadoExclusaoPermanente>
        ExcluirPermanentementeAsync(int id)
    {
        var tarefaEncontrada =
            await _tarefaRepository
                .BuscarIncluindoExcluidasPorIdAsync(id);

        if (tarefaEncontrada is null)
        {
            _logger.LogWarning(
                "Tarefa não encontrada para exclusão permanente. TarefaId={TarefaId}",
                id
            );

            return ResultadoExclusaoPermanente.NaoEncontrada;
        }

        if (tarefaEncontrada.ExcluidaEm is null)
        {
            _logger.LogWarning(
                "Exclusão permanente bloqueada: tarefa ainda ativa. TarefaId={TarefaId}",
                id
            );

            return ResultadoExclusaoPermanente.TarefaAtiva;
        }

        _tarefaRepository.Remover(tarefaEncontrada);
        await _tarefaRepository.SalvarAlteracoesAsync();

        _logger.LogInformation(
            "Tarefa excluída permanentemente. TarefaId={TarefaId}",
            id
        );

        return ResultadoExclusaoPermanente.Sucesso;
    }

    private static string NormalizarSituacaoCriacao(
        string? situacao)
    {
        return string.IsNullOrWhiteSpace(situacao)
            ? "Pendente"
            : situacao.Trim();
    }

    private static bool EstaConcluida(
        string situacao)
    {
        return string.Equals(
            situacao,
            "Concluída",
            StringComparison.OrdinalIgnoreCase
        );
    }

    private static TarefaResponse MapearParaResponse(
        Tarefa tarefa)
    {
        return new TarefaResponse
        {
            Id = tarefa.Id,
            Descricao = tarefa.Descricao,
            Situacao = tarefa.Situacao,
            CriadaEm = tarefa.CriadaEm,
            ModificadaEm = tarefa.ModificadaEm,
            SituacaoAlteradaEm = tarefa.SituacaoAlteradaEm,
            ConcluidaEm = tarefa.ConcluidaEm,
            ExcluidaEm = tarefa.ExcluidaEm
        };
    }
}