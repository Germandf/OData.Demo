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
