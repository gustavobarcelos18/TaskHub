using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoTarefas.DTOs.Responses;
using ProjetoTarefas.Services;

namespace ProjetoTarefas.Controllers;

[ApiController]
[Route("api/health")]
[Authorize]
public sealed class HealthController(
    IConfiguration configuration,
    HealthDiagnosticsService diagnosticsService) : ControllerBase
{
    [HttpGet("detalhes")]
    [ProducesResponseType(typeof(HealthDetalhadoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HealthDetalhadoResponse>> ObterDetalhes(CancellationToken cancellationToken)
    {
        if (!configuration.GetValue<bool>("TechnicalDiagnostics:Enabled"))
            return NotFound();

        return Ok(await diagnosticsService.ObterDetalhesAsync(cancellationToken));
    }
}
