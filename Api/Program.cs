using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDb>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
builder.Services
    .AddControllers(opt =>
        opt.Filters.Add<ODataRequestLoggingFilter>())
    .AddOData(opt =>
        opt.EnableQueryFeatures().AddRouteComponents("", GetEdmModel()));

builder.Services.AddSwaggerGen(c =>
{
    c.DocumentFilter<ODataSwaggerCleanupFilter>();
    c.OperationFilter<ODataQueryOptionsOperationFilter>();
});

var app = builder.Build();

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDb>();
db.Database.EnsureCreated();
if (!db.Customers.Any()) DbSeeder.Seed(db);

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();

static IEdmModel GetEdmModel()
{
    var builder = new ODataConventionModelBuilder();
    const int defaultMaxTop = 100;
    const int defaultPageSize = 100;
    builder.EntitySet<CustomerDto>("Customers").EntityType.Page(maxTopValue: 110, pageSizeValue: 110);
    builder.EntitySet<OrderDto>("Orders").EntityType.Page(maxTopValue: defaultMaxTop, pageSizeValue: defaultPageSize);
    builder.EntitySet<OrderItemDto>("OrderItems").EntityType.Page(maxTopValue: defaultMaxTop, pageSizeValue: defaultPageSize);
    builder.EntitySet<CityDto>("Cities").EntityType.Page(maxTopValue: defaultMaxTop, pageSizeValue: defaultPageSize);
    return builder.GetEdmModel();
}

public class CustomersController(AppDb db) : ODataController
{
    [EnableQuery]
    public IQueryable<CustomerDto> Get() => db.Customers.ProjectTo(DtoProjections.CustomerProjection());

    // Necessary for Power Query to work with OData the wrong way (a lot of individual calls)
    [EnableQuery]
    public IQueryable<OrderDto> GetOrders([FromODataUri] int key) =>
        db.Orders.Where(o => o.CustomerId == key).ProjectTo(DtoProjections.OrderProjection());

    // Necessary for Power Query to work with OData the wrong way (a lot of individual calls)
    [EnableQuery]
    public SingleResult<CityDto> GetCity([FromODataUri] int key) =>
        SingleResult.Create(db.Customers.Where(c => c.Id == key).Select(c => c.City).Select(DtoProjections.CityProjection()));
}

public class OrdersController(AppDb db) : ODataController
{
    [EnableQuery]
    public IQueryable<OrderDto> Get() => db.Orders.ProjectTo(DtoProjections.OrderProjection());

    // Necessary for Power Query to work with OData the wrong way (a lot of individual calls)
    [EnableQuery]
    public IQueryable<OrderItemDto> GetItems([FromODataUri] int key) => db.OrderItems
        .Where(i => i.OrderId == key).ProjectTo(DtoProjections.OrderItemProjection());
}

public class OrderItemsController(AppDb db) : ODataController
{
    [EnableQuery]
    public IQueryable<OrderItemDto> Get() => db.OrderItems.ProjectTo(DtoProjections.OrderItemProjection());
}

public class CitiesController(AppDb db) : ODataController
{
    [EnableQuery]
    public IQueryable<CityDto> Get() => db.Cities.ProjectTo(DtoProjections.CityProjection());
}
