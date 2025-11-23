
using System.ComponentModel.DataAnnotations;

namespace Usuario.Intf.Models
{
    public class UserDto
    {

        [Required]
        public string? Email { get; set; }
        public string? Password { get; set; }



    }
}
