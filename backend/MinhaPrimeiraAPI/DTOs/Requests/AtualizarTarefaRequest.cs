using System.ComponentModel.DataAnnotations;

namespace MinhaPrimeiraAPI.DTOs.Requests;

public class AtualizarTarefaRequest
{
    [Required(ErrorMessage = "A descrição da tarefa é obrigatória.")]
    [MaxLength(200, ErrorMessage = "A descrição da tarefa deve ter no máximo 200 caracteres.")]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "A situação da tarefa é obrigatória.")]
    [MaxLength(30, ErrorMessage = "A situação da tarefa deve ter no máximo 30 caracteres.")]
    public string Situacao { get; set; } = string.Empty;
}
