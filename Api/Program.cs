using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDb>(opt => opt.UseInMemoryDatabase("odata-demo"));
builder.Services
    .AddControllers()
    .AddOData(opt =>
        opt.Select().Filter().Expand().OrderBy().Count().SetMaxTop(100)
           .AddRouteComponents("", GetEdmModel()));

builder.Services.AddSwaggerGen(c =>
{
    c.DocumentFilter<ODataSwaggerCleanupFilter>();
});

var app = builder.Build();

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDb>();
if (!db.Customers.Any()) DbSeeder.Seed(db);

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();

static IEdmModel GetEdmModel()
{
    var builder = new ODataConventionModelBuilder();
    var customers = builder.EntitySet<Customer>("Customers");
    var orders = builder.EntitySet<Order>("Orders");
    var items = builder.EntitySet<OrderItem>("OrderItems");
    customers.HasManyBinding(c => c.Orders, orders);
    orders.HasManyBinding(o => o.Items, items);
    return builder.GetEdmModel();
}

public class CustomersController(AppDb db) : ODataController
{
    [EnableQuery]
    public IQueryable<Customer> Get() => db.Customers
        .Include(x => x.Orders)
            .ThenInclude(o => o.Items);
}

public class OrdersController(AppDb db) : ODataController
{
    [EnableQuery]
    public IQueryable<Order> Get() => db.Orders
        .Include(x => x.Customer)
        .Include(x => x.Items);
}

public class OrderItemsController(AppDb db) : ODataController
{
    [EnableQuery]
    public IQueryable<OrderItem> Get() => db.OrderItems
        .Include(x => x.Order);
}

public class AppDb : DbContext
{
    public AppDb(DbContextOptions<AppDb> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
}

public class Customer
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? City { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

public class Order
{
    public int Id { get; set; }
    public DateTime PlacedAt { get; set; }
    public decimal Total { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public class OrderItem
{
    public int Id { get; set; }
    public required string Sku { get; set; }
    public required string Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = default!;
}
