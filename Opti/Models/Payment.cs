using System.ComponentModel;

namespace Opti.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionExternalId { get; set; }
        public string Gateway { get; set; }
        public string Status { get; set; }
        public ProductionOrder ProductionOrder { get; set; }
        public User User { get; set; }
    }
}
