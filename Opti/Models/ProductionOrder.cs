namespace Opti.Models
{
    public class ProductionOrder
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public int UserId { get; set; }
        public Product Product { get; set; }
        public User User { get; set; }
    }
}
