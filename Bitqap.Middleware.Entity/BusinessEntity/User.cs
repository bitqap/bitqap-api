using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Bitqap.Middleware.Entity.BusinessEntity
{
    public class User
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        [JsonIgnore]
        [Required]
        [Range(6, 20)]
        public string Password { get; set; }
        public DateTime RegisterDate { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Token { get; set; }
    }
}