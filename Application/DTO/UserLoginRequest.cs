using System.ComponentModel.DataAnnotations;

namespace ASP.NET_Hands_on.DTO
{
    public class UserLoginRequest
    {
        [MaxLength(50)]
        public required string UserName { get; set; }
        [MinLength(8), MaxLength(30)]
        public required string Password { get; set; }
    }
}
