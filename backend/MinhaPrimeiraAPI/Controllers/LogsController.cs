using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoTarefas.DTOs.Requests;
using ProjetoTarefas.DTOs.Responses;
using ProjetoTarefas.Services;

namespace ProjetoTarefas.Controllers;

[ApiController]
[Route("api/logs")]
[Authorize]
public sealed class LogsController(IConfiguration configuration, LogService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(LogsPaginadosResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LogsPaginadosResponse>> Listar([FromQuery] ConsultaLogsRequest consulta, CancellationToken cancellationToken)
    {
        if (!configuration.GetValue<bool>("TechnicalDiagnostics:Enabled")) return NotFound();
        try { return Ok(await service.ConsultarAsync(consulta, cancellationToken)); }
        catch (ArgumentException exception) { return Problem(detail: exception.Message, statusCode: StatusCodes.Status400BadRequest, title: "Consulta inválida"); }
    }
}
