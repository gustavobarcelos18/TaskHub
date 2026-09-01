using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProjetoTarefas.DTOs.Requests;
using ProjetoTarefas.DTOs.Responses;
using ProjetoTarefas.Services;

namespace ProjetoTarefas.Controllers;

[ApiController]
[Authorize]
[Route("api/etiquetas")]
public class EtiquetasController(IEtiquetaService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<EtiquetaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EtiquetaResponse>>> Listar(CancellationToken cancellationToken) => Ok(await service.ListarAsync(cancellationToken));
    [HttpPost]
    [ProducesResponseType(typeof(EtiquetaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EtiquetaResponse>> Criar([FromBody] CriarEtiquetaRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var etiqueta = await service.CriarAsync(request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, etiqueta);
        }
        catch (ArgumentException exception)
        {
            return Problem(detail: exception.Message, title: "Etiqueta inválida", statusCode: StatusCodes.Status400BadRequest);
        }
        catch (EtiquetaDuplicadaException exception)
        {
            return Problem(detail: exception.Message, title: "Etiqueta duplicada", statusCode: StatusCodes.Status409Conflict);
        }
    }
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return Problem(detail: "O ID da etiqueta deve ser maior que zero.", title: "ID inválido", statusCode: StatusCodes.Status400BadRequest);
        }

        return await service.ExcluirAsync(id, cancellationToken)
            ? NoContent()
            : Problem(detail: $"Nenhuma etiqueta encontrada com o ID {id}.", title: "Etiqueta não encontrada", statusCode: StatusCodes.Status404NotFound);
    }
}
