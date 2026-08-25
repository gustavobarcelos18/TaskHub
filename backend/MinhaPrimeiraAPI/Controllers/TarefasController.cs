using Microsoft.AspNetCore.Mvc;
using MinhaPrimeiraAPI.DTOs.Requests;
using MinhaPrimeiraAPI.DTOs.Responses;
using MinhaPrimeiraAPI.Services;

namespace MinhaPrimeiraAPI.Controllers;

[ApiController]
[Route("api/tarefas")]
public class TarefasController : ControllerBase
{
    private readonly ITarefaService _tarefaService;
    private readonly ILogger<TarefasController> _logger;

    public TarefasController(
        ITarefaService tarefaService,
        ILogger<TarefasController> logger)
    {
        _tarefaService = tarefaService;
        _logger = logger;
    }

    /// <summary>
    /// Lista todas as tarefas ativas.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(TarefasPaginadasResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TarefasPaginadasResponse>> Listar(
        [FromQuery] ConsultaTarefasRequest consulta,
        CancellationToken cancellationToken)
    {
        try
        {
            var tarefasEncontradas = await _tarefaService.ListarAsync(consulta, cancellationToken);
            return Ok(tarefasEncontradas);
        }
        catch (ArgumentException exception)
        {
            _logger.LogWarning(exception, "Consulta de tarefas rejeitada.");
            return Problem(
                detail: exception.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Consulta inválida"
            );
        }
    }

    /// <summary>
    /// Retorna os indicadores das tarefas ativas.
    /// </summary>
    [HttpGet("resumo")]
    [ProducesResponseType(typeof(ResumoTarefasResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResumoTarefasResponse>> ObterResumo(CancellationToken cancellationToken)
    {
        var resumo = await _tarefaService.ObterResumoAsync(cancellationToken);

        return Ok(resumo);
    }

    /// <summary>
    /// Lista todas as tarefas excluídas logicamente.
    /// </summary>
    [HttpGet("excluidas")]
    [ProducesResponseType(typeof(List<TarefaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TarefaResponse>>> ListarExcluidas(CancellationToken cancellationToken)
    {
        var tarefasEncontradas =
            await _tarefaService.ListarExcluidasAsync(cancellationToken);

        return Ok(tarefasEncontradas);
    }

    /// <summary>
    /// Lista o histÃ³rico de alteraÃ§Ãµes de uma tarefa, inclusive se ela estiver na lixeira.
    /// </summary>
    [HttpGet("{id:int}/historico")]
    [ProducesResponseType(typeof(List<HistoricoTarefaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<HistoricoTarefaResponse>>> ListarHistorico(
        int id,
        CancellationToken cancellationToken)
    {
        var erroId = ValidarId(id);

        if (erroId is not null)
        {
            _logger.LogWarning(
                "Consulta de histÃ³rico rejeitada. TarefaId={TarefaId}. Motivo={Motivo}",
                id,
                erroId
            );

            return Problem(
                detail: erroId,
                statusCode: StatusCodes.Status400BadRequest,
                title: "ID invÃ¡lido"
            );
        }

        var historico = await _tarefaService.ListarHistoricoAsync(id, cancellationToken);

        if (historico is null)
        {
            return Problem(
                detail: $"Nenhuma tarefa encontrada com o ID {id}.",
                statusCode: StatusCodes.Status404NotFound,
                title: "Tarefa nÃ£o encontrada"
            );
        }

        return Ok(historico);
    }

    /// <summary>
    /// Busca uma tarefa ativa pelo ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TarefaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TarefaResponse>> BuscarPorId(int id, CancellationToken cancellationToken)
    {
        var erroId = ValidarId(id);

        if (erroId is not null)
        {
            _logger.LogWarning(
                "Busca de tarefa rejeitada. TarefaId={TarefaId}. Motivo={Motivo}",
                id,
                erroId
            );

            return Problem(
                detail: erroId,
                statusCode: StatusCodes.Status400BadRequest,
                title: "ID inválido"
            );
        }

        var tarefaEncontrada =
            await _tarefaService.BuscarPorIdAsync(id, cancellationToken);

        if (tarefaEncontrada is null)
        {
            return Problem(
                detail: $"Nenhuma tarefa encontrada com o ID {id}.",
                statusCode: StatusCodes.Status404NotFound,
                title: "Tarefa não encontrada"
            );
        }

        return Ok(tarefaEncontrada);
    }

    /// <summary>
    /// Cria uma nova tarefa.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TarefaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TarefaResponse>> Criar(
        [FromBody] CriarTarefaRequest novaTarefa,
        CancellationToken cancellationToken)
    {
        var tarefaCriada =
            await _tarefaService.CriarAsync(novaTarefa, cancellationToken);

        return CreatedAtAction(
            nameof(BuscarPorId),
            new { id = tarefaCriada.Id },
            tarefaCriada
        );
    }

    /// <summary>
    /// Atualiza uma tarefa ativa.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(
        int id,
        [FromBody] AtualizarTarefaRequest dadosAtualizados,
        CancellationToken cancellationToken)
    {
        var erroId = ValidarId(id);

        if (erroId is not null)
        {
            _logger.LogWarning(
                "Atualização rejeitada. TarefaId={TarefaId}. Motivo={Motivo}",
                id,
                erroId
            );

            return Problem(
                detail: erroId,
                statusCode: StatusCodes.Status400BadRequest,
                title: "ID inválido"
            );
        }

        var tarefaAtualizada =
            await _tarefaService.AtualizarAsync(
                id,
                dadosAtualizados,
                cancellationToken
            );

        if (tarefaAtualizada is null)
        {
            return Problem(
                detail: $"Nenhuma tarefa encontrada com o ID {id}.",
                statusCode: StatusCodes.Status404NotFound,
                title: "Tarefa não encontrada"
            );
        }

        return NoContent();
    }

    /// <summary>
    /// Exclui logicamente uma tarefa ativa.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExcluirLogicamente(int id, CancellationToken cancellationToken)
    {
        var erroId = ValidarId(id);

        if (erroId is not null)
        {
            _logger.LogWarning(
                "Exclusão lógica rejeitada. TarefaId={TarefaId}. Motivo={Motivo}",
                id,
                erroId
            );

            return Problem(
                detail: erroId,
                statusCode: StatusCodes.Status400BadRequest,
                title: "ID inválido"
            );
        }

        var excluiu =
            await _tarefaService.ExcluirLogicamenteAsync(id, cancellationToken);

        if (!excluiu)
        {
            return Problem(
                detail: $"Nenhuma tarefa encontrada com o ID {id}.",
                statusCode: StatusCodes.Status404NotFound,
                title: "Tarefa não encontrada"
            );
        }

        return NoContent();
    }

    /// <summary>
    /// Restaura uma tarefa excluída logicamente.
    /// </summary>
    [HttpPatch("{id:int}/restaurar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Restaurar(int id, CancellationToken cancellationToken)
    {
        var erroId = ValidarId(id);

        if (erroId is not null)
        {
            _logger.LogWarning(
                "Restauração rejeitada. TarefaId={TarefaId}. Motivo={Motivo}",
                id,
                erroId
            );

            return Problem(
                detail: erroId,
                statusCode: StatusCodes.Status400BadRequest,
                title: "ID inválido"
            );
        }

        var resultado = await _tarefaService.RestaurarAsync(id, cancellationToken);

        return resultado switch
        {
            ResultadoRestauracao.Sucesso => NoContent(),
            ResultadoRestauracao.NaoEncontrada => Problem(
                detail: $"Nenhuma tarefa encontrada com o ID {id}.",
                statusCode: StatusCodes.Status404NotFound,
                title: "Tarefa não encontrada"
            ),
            ResultadoRestauracao.TarefaAtiva => Problem(
                detail: "A tarefa já está ativa e não pode ser restaurada.",
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflito ao restaurar tarefa"
            ),
            _ => Problem(
                detail: "Ocorreu um erro interno ao processar a requisição.",
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Erro interno do servidor"
            )
        };
    }

    /// <summary>
    /// Exclui permanentemente uma tarefa previamente excluída logicamente.
    /// </summary>
    [HttpDelete("{id:int}/permanente")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ExcluirPermanentemente(int id, CancellationToken cancellationToken)
    {
        var erroId = ValidarId(id);

        if (erroId is not null)
        {
            _logger.LogWarning(
                "Exclusão permanente rejeitada. TarefaId={TarefaId}. Motivo={Motivo}",
                id,
                erroId
            );

            return Problem(
                detail: erroId,
                statusCode: StatusCodes.Status400BadRequest,
                title: "ID inválido"
            );
        }

        var resultado =
            await _tarefaService.ExcluirPermanentementeAsync(id, cancellationToken);

        switch (resultado)
        {
            case ResultadoExclusaoPermanente.Sucesso:
                return NoContent();

            case ResultadoExclusaoPermanente.NaoEncontrada:
                return Problem(
                    detail: $"Nenhuma tarefa encontrada com o ID {id}.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Tarefa não encontrada"
                );

            case ResultadoExclusaoPermanente.TarefaAtiva:
                return Problem(
                    detail: "A tarefa precisa ser excluída logicamente antes da exclusão permanente.",
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Conflito ao excluir tarefa"
                );

            default:
                _logger.LogError(
                    "Resultado inesperado na exclusão permanente. TarefaId={TarefaId}. Resultado={Resultado}",
                    id,
                    resultado
                );

                return Problem(
                    detail: "Ocorreu um erro interno ao processar a requisição.",
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Erro interno do servidor"
                );
        }
    }

    private static string? ValidarId(int id)
    {
        return id <= 0
            ? "O ID da tarefa deve ser maior que zero."
            : null;
    }
}
