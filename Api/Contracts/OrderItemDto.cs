public class OrderItemDto
{
    public int Id { get; set; }
    public required string Sku { get; set; }
    public required string Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int OrderId { get; set; }
}
