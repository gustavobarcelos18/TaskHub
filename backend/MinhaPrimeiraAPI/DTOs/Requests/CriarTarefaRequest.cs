using System.ComponentModel.DataAnnotations;

namespace MinhaPrimeiraAPI.DTOs.Requests;

public class CriarTarefaRequest
{
    [Required(ErrorMessage = "A descrição da tarefa é obrigatória.")]
    [MaxLength(200, ErrorMessage = "A descrição da tarefa deve ter no máximo 200 caracteres.")]
    public string Descricao { get; set; } = string.Empty;

    [MaxLength(4000, ErrorMessage = "As observações da tarefa devem ter no máximo 4000 caracteres.")]
    public string? Observacoes { get; set; }

    [MaxLength(30, ErrorMessage = "A situação da tarefa deve ter no máximo 30 caracteres.")]
    [RegularExpression(
        @"^\s*(?i:pendente|em andamento|concluída)\s*$",
        ErrorMessage = "A situação da tarefa é inválida."
    )]
    public string? Situacao { get; set; }

    [MaxLength(10, ErrorMessage = "A prioridade da tarefa deve ter no m\u00e1ximo 10 caracteres.")]
    [RegularExpression(
        @"^\s*(?i:baixa|media|alta)\s*$",
        ErrorMessage = "A prioridade da tarefa \u00e9 inv\u00e1lida."
    )]
    public string? Prioridade { get; set; }

    public DateOnly? DataVencimento { get; set; }

    public int? ProjetoId { get; set; }

    public List<int> EtiquetaIds { get; set; } = [];
}
