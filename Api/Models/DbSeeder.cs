static class DbSeeder
{
    public static void Seed(AppDb db)
    {
        db.Customers.AddRange(Enumerable.Range(1, 3).Select(c => new Customer
        {
            Name = $"Customer {c}",
            City = c % 2 == 0 ? "Buenos Aires" : "Córdoba",
            Orders = Enumerable.Range(1, 2).Select(o =>
            {
                var items = Enumerable.Range(1, 3).Select(i =>
                {
                    return new OrderItem
                    {
                        Sku = $"SKU-{c}{o}{i}",
                        Description = $"Item {i} of Order {o} for Customer {c}",
                        Quantity = Random.Shared.Next(1, 4),
                        UnitPrice = Random.Shared.Next(10, 100)
                    };
                }).ToList();
                return new Order
                {
                    PlacedAt = DateTime.UtcNow.AddDays(-(c * 3 + o)),
                    Items = items,
                    Total = items.Sum(x => x.Quantity * x.UnitPrice)
                };
            }).ToList()
        }));
        db.SaveChanges();
    }
}
