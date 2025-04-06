namespace YummyFood.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; }


    }
    public enum OrderStatus
    {
        Placed,
        Processing,
        Shipped,
        Delivered,
        Canceled
    }
    public class OrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int FoodId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}