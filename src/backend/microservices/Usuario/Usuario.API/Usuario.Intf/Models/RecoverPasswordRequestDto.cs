using System.ComponentModel.DataAnnotations;

namespace Usuario.Intf.Models
{
  
    /// DTO usado para solicitar um link de recuperação de senha (apenas o e-mail).
    public class RecoverPasswordRequestDto
    {
        [Required(ErrorMessage = "O E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        public string? Email { get; set; }
    }
}
