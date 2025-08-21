public class OrderItemDto
{
    public required int Id { get; set; }
    public required string Sku { get; set; }
    public required string Description { get; set; }
    public required int Quantity { get; set; }
    public required decimal UnitPrice { get; set; }
    public required int OrderId { get; set; }
}
