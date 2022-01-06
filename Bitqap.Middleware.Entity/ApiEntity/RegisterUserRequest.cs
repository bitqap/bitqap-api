using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Bitqap.Middleware.Entity.ApiEntity
{
    public  class RegisterUserRequest
    {
        public string Username { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        [Required]
        [Range(6, 20)]
        public string Password { get; set; }
    }
}
