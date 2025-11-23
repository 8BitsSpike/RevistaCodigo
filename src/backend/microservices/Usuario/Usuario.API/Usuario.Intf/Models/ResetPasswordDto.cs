using System.ComponentModel.DataAnnotations;

namespace Usuario.Intf.Models
{

    /// DTO usado para redefinir a senha usando o token de segurança.
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "O ID do usuário é obrigatório.")]
        public string? UserId { get; set; }

        [Required(ErrorMessage = "O token de segurança é obrigatório.")]
        public string? Token { get; set; }

        [Required(ErrorMessage = "A nova senha é obrigatória.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 100 caracteres.")]
        public string? NewPassword { get; set; }
    }
}