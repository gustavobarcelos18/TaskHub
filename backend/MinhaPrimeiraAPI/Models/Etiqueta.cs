namespace MinhaPrimeiraAPI.Models;

public class Etiqueta
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string NomeNormalizado { get; set; } = string.Empty;

    public ICollection<Tarefa> Tarefas { get; set; } = new List<Tarefa>();
}
