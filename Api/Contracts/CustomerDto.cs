public class CustomerDto
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required int CityId { get; set; }
    public required CityDto City { get; set; }
    public required ICollection<OrderDto> Orders { get; set; }
}
