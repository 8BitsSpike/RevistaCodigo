
using System.ComponentModel.DataAnnotations;

namespace Usuario.Intf.Models
{
    public class AuthenticateDto
    {

        [Required]
        public string Email { get; set; }
        public string Password { get; set; }



    }
}
