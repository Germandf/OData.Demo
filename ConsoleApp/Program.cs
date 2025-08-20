using ConsoleApp.OData.Client;
using Microsoft.OData.Client;
using System.Text.Json;

var serviceRoot = new Uri("https://localhost:7007/");
var ctx = new Container(serviceRoot);
ctx.Format.UseJson();
ctx.MergeOption = MergeOption.NoTracking;

var customersQuery1 = ctx.Customers
    .Expand("Orders($expand=Items)")
    .Where(c => c.Name == "Customer 1")
    .Take(50);
var customers1 = await ((DataServiceQuery<CustomerDto>)customersQuery1).ExecuteAsync();
Console.WriteLine("Customers using handwritten Expands:");
Console.WriteLine(JsonSerializer.Serialize(customers1, new JsonSerializerOptions { WriteIndented = true }));

var customersQuery2 = ctx.Customers
    .Expand(c => c.Orders)
    .Where(c => c.Name == "Customer 1")
    .Take(50);
var customers2 = await ((DataServiceQuery<CustomerDto>)customersQuery2).ExecuteAsync();
Console.WriteLine("Customers using lambda Expands:");
Console.WriteLine(JsonSerializer.Serialize(customers2, new JsonSerializerOptions { WriteIndented = true }));

var customersQuery3 = ctx.Customers
    .Where(c => c.Name == "Customer 1")
    .Take(50);
var customers3 = await ((DataServiceQuery<CustomerDto>)customersQuery3).ExecuteAsync();
Console.WriteLine("Customers without Expands:");
Console.WriteLine(JsonSerializer.Serialize(customers3, new JsonSerializerOptions { WriteIndented = true }));