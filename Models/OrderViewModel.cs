namespace ArtGalleryFinal.Models
{
    public class OrderViewModel
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public DateOnly Date { get; set; }
        public int OrderAmount { get; set; }
        public string PaymentStatus { get; set; }
        public int Discount { get; set; }
        public string ArtName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int ShipmentCharges { get; set; }
        public string OrderStatus { get; set; }
        public int TotalAmount { get; set; }
        public string CustomerName { get; set; }
        public string ImageUrl { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }


        
    }
}
