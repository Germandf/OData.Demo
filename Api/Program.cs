using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDb>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
builder.Services.AddControllers().AddOData(opt => opt.EnableQueryFeatures().AddRouteComponents("", GetEdmModel()));
builder.Services.AddSwaggerGen(c =>
{
    c.DocumentFilter<ODataSwaggerCleanupFilter>();
    c.OperationFilter<ODataQueryOptionsOperationFilter>();
    c.DocumentFilter<ApiKeySecurityDocumentFilter>();
});

var app = builder.Build();

app.EnsureDbCreatedAndSeeded();
app.UseRequestResponseLogging();
app.UseApiKey("asd123");
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();

static IEdmModel GetEdmModel()
{
    var builder = new ODataConventionModelBuilder();
    var defaultPage = new ODataPage { MaxTop = 100, PageSize = 100 };
    builder.EntitySet<CustomerDto>("Customers").Page(new() { MaxTop = 110, PageSize = 110 });
    builder.EntitySet<OrderDto>("Orders").Page(defaultPage);
    builder.EntitySet<OrderItemDto>("OrderItems").Page(defaultPage);
    builder.EntitySet<CityDto>("Cities").Page(defaultPage);
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

public partial class Program { }
