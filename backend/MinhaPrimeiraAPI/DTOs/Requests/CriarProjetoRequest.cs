using System.ComponentModel.DataAnnotations;

namespace ProjetoTarefas.DTOs.Requests;

public class CriarProjetoRequest
{
    [Required(ErrorMessage = "O nome do projeto é obrigatório.")]
    [MaxLength(100, ErrorMessage = "O nome do projeto deve ter no máximo 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;
}
