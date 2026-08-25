using System.ComponentModel.DataAnnotations;

namespace MinhaPrimeiraAPI.DTOs.Requests;

public class CriarEtiquetaRequest
{
    [Required(ErrorMessage = "O nome da etiqueta é obrigatório.")]
    [MaxLength(50, ErrorMessage = "O nome da etiqueta deve ter no máximo 50 caracteres.")]
    public string Nome { get; set; } = string.Empty;
}
