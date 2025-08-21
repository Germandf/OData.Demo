using Bogus;

public static class DbSeeder
{
    public static void Seed(AppDb db, int customerCount = 1000, int? seed = 1337)
    {
        if (db.Customers.Any()) return;

        Randomizer.Seed = new Random(seed ?? Environment.TickCount);

        var cityFaker = new Faker<City>("es")
            .RuleFor(c => c.Name, f => f.Address.City());

        var itemFaker = new Faker<OrderItem>("es")
            .RuleFor(i => i.Sku, f => $"SKU-{f.IndexFaker:D7}")
            .RuleFor(i => i.Description, f => f.Commerce.ProductName())
            .RuleFor(i => i.Quantity, f => f.Random.Number(1, 5))
            .RuleFor(i => i.UnitPrice, f => Math.Round(f.Random.Decimal(2500m, 150000m), 2));

        var orderFaker = new Faker<Order>("es")
            .RuleFor(o => o.PlacedAt, f => f.Date.Between(DateTime.UtcNow.AddMonths(-18), DateTime.UtcNow))
            .RuleFor(o => o.Items, f => itemFaker.Generate(2).ToList())
            .FinishWith((f, o) => o.Total = o.Items.Sum(it => it.UnitPrice * it.Quantity));

        var customerFaker = new Faker<Customer>("es")
            .RuleFor(c => c.Name, f => f.Person.FullName)
            .RuleFor(c => c.City, f => cityFaker.Generate())
            .RuleFor(c => c.Orders, f => orderFaker.Generate(2).ToList());

        var customers = customerFaker.Generate(customerCount);
        db.Customers.AddRange(customers);
        db.SaveChanges();
    }
}