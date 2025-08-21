public class CustomerDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int CityId { get; set; }
    public CityDto? City { get; set; }
    public ICollection<OrderDto>? Orders { get; set; }
}
