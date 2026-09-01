using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using ProjetoTarefas.DTOs.Requests;
using ProjetoTarefas.DTOs.Responses;
using ProjetoTarefas.Models;

namespace ProjetoTarefas.Controllers;

[ApiController]
[Route("api/autenticacao")]
public class AutenticacaoController(
    UserManager<Usuario> userManager,
    SignInManager<Usuario> signInManager,
    IAntiforgery antiforgery) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("antiforgery")]
    [ProducesResponseType(typeof(TokenAntiforgeryResponse), StatusCodes.Status200OK)]
    public ActionResult<TokenAntiforgeryResponse> ObterTokenAntiforgery()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new TokenAntiforgeryResponse { Token = tokens.RequestToken! });
    }

    [AllowAnonymous]
    [HttpPost("cadastro")]
    [ProducesResponseType(typeof(UsuarioAutenticadoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioAutenticadoResponse>> Cadastrar(
        [FromBody] CadastrarUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email?.Trim();

        if (string.IsNullOrWhiteSpace(email))
        {
            return Problem(
                detail: "O e-mail é obrigatório.",
                title: "Cadastro inválido",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var senha = request.Senha;

        if (senha is null)
        {
            return Problem(
                detail: "A senha é obrigatória.",
                title: "Cadastro inválido",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var usuario = new Usuario { UserName = email, Email = email };
        var resultado = await userManager.CreateAsync(usuario, senha);

        if (!resultado.Succeeded)
        {
            if (resultado.Errors.Any(erro => erro.Code is "DuplicateEmail" or "DuplicateUserName"))
            {
                return Problem(
                    detail: "Já existe uma conta cadastrada com este e-mail.",
                    title: "Cadastro indisponível",
                    statusCode: StatusCodes.Status409Conflict);
            }

            return Problem(
                detail: string.Join(" ", resultado.Errors.Select(erro => erro.Description)),
                title: "Cadastro inválido",
                statusCode: StatusCodes.Status400BadRequest);
        }

        await signInManager.SignInAsync(usuario, isPersistent: false);

        return CreatedAtAction(nameof(ObterSessao), CriarResposta(usuario));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(UsuarioAutenticadoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UsuarioAutenticadoResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email?.Trim();

        if (string.IsNullOrWhiteSpace(email))
        {
            return Problem(
                detail: "O e-mail é obrigatório.",
                title: "Login inválido",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (request.Senha is null)
        {
            return Problem(
                detail: "A senha é obrigatória.",
                title: "Login inválido",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var usuario = await userManager.FindByEmailAsync(email);

        if (usuario is null)
        {
            return Problem(
                detail: "E-mail ou senha inválidos.",
                title: "Não autenticado",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var resultado = await signInManager.PasswordSignInAsync(
            usuario,
            request.Senha,
            isPersistent: false,
            lockoutOnFailure: true
        );

        if (!resultado.Succeeded)
        {
            return Problem(
                detail: "E-mail ou senha inválidos.",
                title: "Não autenticado",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        return Ok(CriarResposta(usuario));
    }

    [Authorize]
    [HttpGet("sessao")]
    [ProducesResponseType(typeof(UsuarioAutenticadoResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UsuarioAutenticadoResponse>> ObterSessao()
    {
        var usuario = await userManager.GetUserAsync(User);

        return usuario is null
            ? Unauthorized()
            : Ok(CriarResposta(usuario));
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return NoContent();
    }

    private static UsuarioAutenticadoResponse CriarResposta(Usuario usuario) => new()
    {
        Id = usuario.Id,
        Email = usuario.Email ?? string.Empty
    };
}

public sealed class TokenAntiforgeryResponse
{
    public required string Token { get; init; }
}
