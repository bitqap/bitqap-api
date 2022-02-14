using System.ComponentModel.DataAnnotations;

namespace Bitqap.Middleware.Entity.ApiEntity
{
    public class RegisterUserRequest
    {
        public string Username { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        [Required]
        [Range(6, 20)]
        public string Password { get; set; }
    }
}
