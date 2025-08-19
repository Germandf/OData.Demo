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
    c.OperationFilter<ODataQueryOptionsOperationFilter>();
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
    builder.EntitySet<CustomerDto>("Customers");
    builder.EntitySet<OrderDto>("Orders");
    builder.EntitySet<OrderItemDto>("OrderItems");
    return builder.GetEdmModel();
}

public class CustomersController(AppDb db) : ODataController
{
    [EnableQuery]
    public IQueryable<CustomerDto> Get() =>
        db.Customers.AsNoTracking().Select(c => new CustomerDto
        {
            Id = c.Id,
            Name = c.Name,
            City = c.City,
            Orders = c.Orders.Select(o => new OrderDto
            {
                Id = o.Id,
                PlacedAt = o.PlacedAt,
                Total = o.Total,
                CustomerId = o.CustomerId,
                Customer = new CustomerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    City = c.City
                },
                Items = o.Items.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    Sku = i.Sku,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    OrderId = i.OrderId,
                }).ToList()
            }).ToList()
        });
}

public class OrdersController(AppDb db) : ODataController
{
    [EnableQuery]
    public IQueryable<OrderDto> Get() =>
        db.Orders.AsNoTracking().Select(o => new OrderDto
        {
            Id = o.Id,
            PlacedAt = o.PlacedAt,
            Total = o.Total,
            CustomerId = o.CustomerId,
            Customer = new CustomerDto
            {
                Id = o.Customer.Id,
                Name = o.Customer.Name,
                City = o.Customer.City
            },
            Items = o.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                Sku = i.Sku,
                Description = i.Description,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                OrderId = i.OrderId,
            }).ToList()
        });
}

public class OrderItemsController(AppDb db) : ODataController
{
    [EnableQuery]
    public IQueryable<OrderItemDto> Get() =>
        db.OrderItems.AsNoTracking().Select(i => new OrderItemDto
        {
            Id = i.Id,
            Sku = i.Sku,
            Description = i.Description,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            OrderId = i.OrderId,
        });
}