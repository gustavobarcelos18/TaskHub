namespace ProjetoTarefas.Services;

public sealed class ProjetoDuplicadoException : Exception
{
    public ProjetoDuplicadoException() : base("Já existe um projeto com este nome.") { }
}
