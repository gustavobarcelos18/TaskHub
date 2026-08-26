using ProjetoTarefas.Services;

namespace ProjetoTarefas.Tests.Fakes;

internal sealed class UsuarioAtualFake(string id = "usuario-teste") : IUsuarioAtual
{
    public string Id { get; } = id;
}
