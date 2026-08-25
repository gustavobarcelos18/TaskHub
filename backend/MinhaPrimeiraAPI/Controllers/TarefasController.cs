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
    [ProducesResponseType(typeof(List<TarefaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TarefaResponse>>> Listar()
    {
        var tarefasEncontradas =
            await _tarefaService.ListarAsync();

        return Ok(tarefasEncontradas);
    }

    /// <summary>
    /// Busca uma tarefa ativa pelo ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TarefaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TarefaResponse>> BuscarPorId(int id)
    {
        var erroId = ValidarId(id);

        if (erroId is not null)
        {
            _logger.LogWarning(
                "Busca de tarefa rejeitada. TarefaId={TarefaId}. Motivo={Motivo}",
                id,
                erroId
            );

            return BadRequest(erroId);
        }

        var tarefaEncontrada =
            await _tarefaService.BuscarPorIdAsync(id);

        if (tarefaEncontrada is null)
        {
            return NotFound(
                $"Nenhuma tarefa encontrada com o ID {id}."
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
        [FromBody] CriarTarefaRequest novaTarefa)
    {
        var tarefaCriada =
            await _tarefaService.CriarAsync(novaTarefa);

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
        [FromBody] AtualizarTarefaRequest dadosAtualizados)
    {
        var erroId = ValidarId(id);

        if (erroId is not null)
        {
            _logger.LogWarning(
                "Atualização rejeitada. TarefaId={TarefaId}. Motivo={Motivo}",
                id,
                erroId
            );

            return BadRequest(erroId);
        }

        var tarefaAtualizada =
            await _tarefaService.AtualizarAsync(
                id,
                dadosAtualizados
            );

        if (tarefaAtualizada is null)
        {
            return NotFound(
                $"Nenhuma tarefa encontrada com o ID {id}."
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
    public async Task<IActionResult> ExcluirLogicamente(int id)
    {
        var erroId = ValidarId(id);

        if (erroId is not null)
        {
            _logger.LogWarning(
                "Exclusão lógica rejeitada. TarefaId={TarefaId}. Motivo={Motivo}",
                id,
                erroId
            );

            return BadRequest(erroId);
        }

        var excluiu =
            await _tarefaService.ExcluirLogicamenteAsync(id);

        if (!excluiu)
        {
            return NotFound(
                $"Nenhuma tarefa encontrada com o ID {id}."
            );
        }

        return NoContent();
    }

    /// <summary>
    /// Exclui permanentemente uma tarefa previamente excluída logicamente.
    /// </summary>
    [HttpDelete("{id:int}/permanente")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ExcluirPermanentemente(int id)
    {
        var erroId = ValidarId(id);

        if (erroId is not null)
        {
            _logger.LogWarning(
                "Exclusão permanente rejeitada. TarefaId={TarefaId}. Motivo={Motivo}",
                id,
                erroId
            );

            return BadRequest(erroId);
        }

        var resultado =
            await _tarefaService.ExcluirPermanentementeAsync(id);

        switch (resultado)
        {
            case ResultadoExclusaoPermanente.Sucesso:
                return NoContent();

            case ResultadoExclusaoPermanente.NaoEncontrada:
                return NotFound(
                    $"Nenhuma tarefa encontrada com o ID {id}."
                );

            case ResultadoExclusaoPermanente.TarefaAtiva:
                return Conflict(
                    "A tarefa precisa ser excluída logicamente antes da exclusão permanente."
                );

            default:
                _logger.LogError(
                    "Resultado inesperado na exclusão permanente. TarefaId={TarefaId}. Resultado={Resultado}",
                    id,
                    resultado
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro inesperado durante a exclusão permanente."
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
