using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bitqap.Middleware.Entity.BusinessEntity
{
    public class ErrorDetails
    {
        public string? StatusCode { get; set; }
        public string? Message { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Errors { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
