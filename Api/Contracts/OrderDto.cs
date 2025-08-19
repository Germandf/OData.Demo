public class OrderDto
{
    public int Id { get; set; }
    public DateTime PlacedAt { get; set; }
    public decimal Total { get; set; }
    public int CustomerId { get; set; }
    public required CustomerDto Customer { get; set; }
    public ICollection<OrderItemDto>? Items { get; set; }
}
