using BlazorApp.Components;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped(sp =>
{
    var ctx = new OData.Demo.OData.Client.Container(new Uri("https://localhost:7007/"));
    ctx.Format.UseJson();
    ctx.MergeOption = Microsoft.OData.Client.MergeOption.NoTracking;
    ctx.Configurations.RequestPipeline.OnMessageCreating = args => new Microsoft.OData.Client.HttpClientRequestMessage(args);
    ctx.SendingRequest2 += (sender, e) =>
    {
        e.RequestMessage.SetHeader("ApiKey", "asd123");
    };
    return ctx;
});

builder.Services.AddHttpClient();
builder.Services.AddFluentUIComponents();
builder.Services.AddDataGridODataAdapter();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
