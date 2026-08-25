using System.ComponentModel.DataAnnotations;

namespace MinhaPrimeiraAPI.DTOs.Requests;

public class AtualizarTarefaRequest
{
    [Required(ErrorMessage = "A descrição da tarefa é obrigatória.")]
    [MaxLength(200, ErrorMessage = "A descrição da tarefa deve ter no máximo 200 caracteres.")]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "A situação da tarefa é obrigatória.")]
    [MaxLength(30, ErrorMessage = "A situação da tarefa deve ter no máximo 30 caracteres.")]
    [RegularExpression(
        @"^\s*(?i:pendente|em andamento|concluída)\s*$",
        ErrorMessage = "A situação da tarefa é inválida."
    )]
    public string Situacao { get; set; } = string.Empty;

    [Required(ErrorMessage = "A prioridade da tarefa \u00e9 obrigat\u00f3ria.")]
    [MaxLength(10, ErrorMessage = "A prioridade da tarefa deve ter no m\u00e1ximo 10 caracteres.")]
    [RegularExpression(
        @"^\s*(?i:baixa|media|alta)\s*$",
        ErrorMessage = "A prioridade da tarefa \u00e9 inv\u00e1lida."
    )]
    public string Prioridade { get; set; } = string.Empty;

    public DateOnly? DataVencimento { get; set; }
}
