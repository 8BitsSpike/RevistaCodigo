using System.ComponentModel.DataAnnotations;

namespace Usuario.Intf.Models
{
    public class UsuarioDto
    {
      
        public string? Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Sobrenome { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }
        [Required]
        public string? PasswordConfirm { get; set; }
    }
}
