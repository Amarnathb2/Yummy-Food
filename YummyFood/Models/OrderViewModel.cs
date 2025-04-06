namespace YummyFood.Models
{
    public class OrderViewModel
    {
        public List<CartItem> Items { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
