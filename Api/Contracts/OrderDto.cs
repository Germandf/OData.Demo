public class OrderDto
{
    public required int Id { get; set; }
    public required DateTime PlacedAt { get; set; }
    public required decimal Total { get; set; }
    public required int CustomerId { get; set; }
    public required ICollection<OrderItemDto> Items { get; set; }
}
