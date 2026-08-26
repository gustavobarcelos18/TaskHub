using System.Security.Claims;

namespace ProjetoTarefas.Services;

public sealed class UsuarioAtual(IHttpContextAccessor httpContextAccessor) : IUsuarioAtual
{
    public string Id => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Nenhum usuário autenticado foi encontrado na requisição.");
}
