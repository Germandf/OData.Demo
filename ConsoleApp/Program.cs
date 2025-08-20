using ConsoleApp.OData.Client;
using Microsoft.OData.Client;
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

var customersWithStringExpand = await ctx.Customers
    .Expand("Orders($expand=Items)")
    .ExecuteAsync();

var customersWithLambdaExpand = await ctx.Customers
    .Expand(c => c.Orders)
    .ExecuteAsync();

var customersWithoutExpand = await ctx.Customers
    .ExecuteAsync();

WriteJsonInConsole(customersWithStringExpand, "Expand with raw string");
WriteJsonInConsole(customersWithLambdaExpand, "Expand with lambda");
WriteJsonInConsole(customersWithoutExpand, "No expand");

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