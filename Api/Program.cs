using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDb>(opt => opt.UseInMemoryDatabase("odata-demo"));
builder.Services
    .AddControllers(opt =>
        opt.Filters.Add<ODataRequestLoggingFilter>())
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
    builder.EntitySet<CustomerDto>("Customers").EntityType.Page(maxTopValue: 100, pageSizeValue: 100);
    builder.EntitySet<OrderDto>("Orders").EntityType.Page(maxTopValue: 100, pageSizeValue: 100);
    builder.EntitySet<OrderItemDto>("OrderItems").EntityType.Page(maxTopValue: 100, pageSizeValue: 100);
    builder.EntitySet<CityDto>("Cities").EntityType.Page(maxTopValue: 100, pageSizeValue: 100);
    return builder.GetEdmModel();
}

public class CustomersController(AppDb db) : ODataController
{
    [EnableQuery]
    public IQueryable<CustomerDto> Get() => db.Customers.ProjectTo(DtoProjections.CustomerProjection());
}

public class OrdersController(AppDb db) : ODataController
{
    [EnableQuery]
    public IQueryable<OrderDto> Get() => db.Orders.ProjectTo(DtoProjections.OrderProjection());
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
