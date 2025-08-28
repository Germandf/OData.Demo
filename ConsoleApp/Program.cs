using Microsoft.OData.Client;
using OData.Demo.OData.Client;
using OData.QueryBuilder.Builders;
using Spectre.Console;
using Spectre.Console.Json;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

Console.OutputEncoding = Encoding.UTF8;
var serviceRoot = new Uri("https://localhost:7007/");
var ctx = new Container(serviceRoot);
ctx.Format.UseJson();
ctx.MergeOption = MergeOption.NoTracking;
ctx.SendingRequest2 += (sender, e) =>
{
    e.RequestMessage.SetHeader("ApiKey", "asd123");
};

var customersWithStringExpand = await ((DataServiceQuery<CustomerDto>)ctx.Customers
    .Expand("City,Orders($expand=Items)")
    .Take(1))
    .ExecuteAsync();

var customersWithLambdaExpand = await ((DataServiceQuery<CustomerDto>)ctx.Customers
    .Expand(c => c.City)
    .Expand(c => c.Orders)
    .Take(1))
    .ExecuteAsync();

var customersWithoutExpand = await ((DataServiceQuery<CustomerDto>)ctx.Customers
    .Take(1))
    .ExecuteAsync();

var uri = new ODataQueryBuilder<Container>(serviceRoot.ToString())
    .For<CustomerDto>(c => c.Customers)
    .ByList()
    .Expand(x => x.For<CityDto>(c => c.City))
    .Expand(x => x.For<OrderDto>(c => c.Orders).Expand(y => y.For<OrderItemDto>(i => i.Items)))
    .Top(1)
    .ToUri();
var customersWithODataQueryBuilder = await ctx.ExecuteAsync<CustomerDto>(uri);

var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Add("ApiKey", "asd123");
var apiClient = new OData.Demo.OpenApi.Client.ApiClient("https://localhost:7007/", httpClient);
var customersWithOpenApiGeneratedClient = (await apiClient.GetCustomersAsync(
    expand: "City,Orders($expand=Items)",
    top: 1))
    .Value;

WriteJsonInConsole(customersWithStringExpand, "Microsoft.OData.Client - Expand with raw string");
WriteJsonInConsole(customersWithLambdaExpand, "Microsoft.OData.Client - Expand with lambda");
WriteJsonInConsole(customersWithoutExpand, "Microsoft.OData.Client - No expand");
WriteJsonInConsole(customersWithODataQueryBuilder, "ODataQueryBuilder - Expand with lambda");
WriteJsonInConsole(customersWithOpenApiGeneratedClient, "NSwag.ApiDescription.Client - Expand with raw string");

static void WriteJsonInConsole<T>(IEnumerable<T> items, string title)
{
    AnsiConsole.Write(new Rule($"[black on cyan bold] {Markup.Escape(title)} [/]").RuleStyle("grey"));
    var json = JsonSerializer.Serialize(items, new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });
    AnsiConsole.Write(new Panel(new JsonText(json)).Expand().Border(BoxBorder.Rounded).BorderColor(Color.CadetBlue));
}
