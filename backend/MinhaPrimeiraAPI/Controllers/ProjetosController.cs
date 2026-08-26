using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProjetoTarefas.DTOs.Requests;
using ProjetoTarefas.DTOs.Responses;
using ProjetoTarefas.Services;

namespace ProjetoTarefas.Controllers;

[ApiController]
[Authorize]
[Route("api/projetos")]
public class ProjetosController(IProjetoService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<ProjetoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProjetoResponse>>> Listar(CancellationToken cancellationToken) => Ok(await service.ListarAsync(cancellationToken));
    [HttpPost]
    [ProducesResponseType(typeof(ProjetoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProjetoResponse>> Criar([FromBody] CriarProjetoRequest request, CancellationToken cancellationToken)
    {
        try { var projeto = await service.CriarAsync(request, cancellationToken); return Created($"api/projetos/{projeto.Id}", projeto); }
        catch (ArgumentException exception) { return Problem(detail: exception.Message, title: "Projeto inválido", statusCode: StatusCodes.Status400BadRequest); }
        catch (ProjetoDuplicadoException exception) { return Problem(detail: exception.Message, title: "Projeto duplicado", statusCode: StatusCodes.Status409Conflict); }
    }
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(int id, CancellationToken cancellationToken)
    {
        if (id <= 0) return Problem(detail: "O ID do projeto deve ser maior que zero.", title: "ID inválido", statusCode: StatusCodes.Status400BadRequest);
        return await service.ExcluirAsync(id, cancellationToken) ? NoContent() : Problem(detail: $"Nenhum projeto encontrado com o ID {id}.", title: "Projeto não encontrado", statusCode: StatusCodes.Status404NotFound);
    }
}
