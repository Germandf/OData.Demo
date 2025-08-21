public class Customer
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int CityId { get; set; }
    public City City { get; set; } = default!;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
