using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDb>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
builder.Services.AddControllers().AddOData(opt =>
    opt.EnableQueryFeatures().SetMaxTop(100).AddRouteComponents("", GetEdmModel()));
builder.Services.AddSwaggerGen(c =>
{
    c.DocumentFilter<ODataSwaggerCleanupFilter>();
    c.OperationFilter<ODataQueryOptionsOperationFilter>();
    c.OperationFilter<ODataCollectionResponseOperationFilter>();
    c.OperationFilter<ODataOperationIdFilter>();
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
    builder.EntitySet<CustomerDto>("Customers");
    builder.EntitySet<OrderDto>("Orders");
    builder.EntitySet<OrderItemDto>("OrderItems");
    builder.EntitySet<CityDto>("Cities");
    return builder.GetEdmModel();
}

public class CustomersController(AppDb db) : ODataController
{
    [EnableQuery(PageSize = 110, MaxTop = 110)]
    public IQueryable<CustomerDto> GetCustomers() => db.Customers.ProjectTo(DtoProjections.CustomerProjection());
}

[Route("Customers"), Tags("Customers")]
public class CustomersCustomController(AppDb db) : ControllerBase
{
    [HttpPost("", Name = nameof(CreateCustomer))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomerDto))]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        var entity = new Customer
        {
            Name = request.Name,
            CityId = request.CityId,
        };
        db.Customers.Add(entity);
        await db.SaveChangesAsync();
        var dto = await db.Customers.Where(c => c.Id == entity.Id).ProjectTo(DtoProjections.CustomerProjection()).FirstAsync();
        return Ok(dto);
    }
}

public class OrdersController(AppDb db) : ODataController
{
    [EnableQuery(PageSize = 100)]
    public IQueryable<OrderDto> Get() => db.Orders.ProjectTo(DtoProjections.OrderProjection());
}

public class OrderItemsController(AppDb db) : ODataController
{
    [EnableQuery(PageSize = 100)]
    public IQueryable<OrderItemDto> Get() => db.OrderItems.ProjectTo(DtoProjections.OrderItemProjection());
}

public class CitiesController(AppDb db) : ODataController
{
    [EnableQuery(PageSize = 100)]
    public IQueryable<CityDto> Get() => db.Cities.ProjectTo(DtoProjections.CityProjection());
}

public partial class Program { }
