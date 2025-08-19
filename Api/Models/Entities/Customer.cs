public class Customer
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? City { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
