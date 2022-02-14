using System.ComponentModel.DataAnnotations;

namespace Bitqap.Middleware.Entity.BusinessEntity
{
    public class Account
    {
        [Required]
        public long Id { get; set; }
        [Required]
        public long UserId { get; set; }
        [Required]
        public string AccountKey { get; set; }
        public string AccountName { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
