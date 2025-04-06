namespace YummyFood.Models
{
    public class OrderSummaryViewModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public List<OrderedItem> Items { get; set; }
    }

    public class OrderedItem
    {
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
