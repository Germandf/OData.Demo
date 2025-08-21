using LinqKit;
using System.Linq.Expressions;

public static class DtoProjections
{
    public static Expression<Func<City, CityDto>> CityProjection() =>
        c => new CityDto
        {
            Id = c.Id,
            Name = c.Name
        };

    public static Expression<Func<OrderItem, OrderItemDto>> OrderItemProjection() =>
        i => new OrderItemDto
        {
            Id = i.Id,
            Sku = i.Sku,
            Description = i.Description,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            OrderId = i.OrderId
        };

    public static Expression<Func<Order, OrderDto>> OrderProjection() =>
        o => new OrderDto
        {
            Id = o.Id,
            PlacedAt = o.PlacedAt,
            Total = o.Total,
            CustomerId = o.CustomerId,
            Items = o.Items
                .AsQueryable()
                .Select(i => OrderItemProjection().Invoke(i))
                .ToList()
        };

    public static Expression<Func<Customer, CustomerDto>> CustomerProjection(bool cityRequired = false) =>
        c => new CustomerDto
        {
            Id = c.Id,
            Name = c.Name,
            CityId = c.CityId,
            City = CityProjection().Invoke(c.City),
            Orders = c.Orders
                .AsQueryable()
                .Select(o => OrderProjection().Invoke(o))
                .ToList()
        };
}
