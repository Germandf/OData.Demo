using BlazorApp.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped(sp =>
{
    var ctx = new BlazorApp.OData.Client.Container(new Uri("https://localhost:7007/"));
    ctx.Format.UseJson();
    ctx.MergeOption = Microsoft.OData.Client.MergeOption.NoTracking;
    ctx.Configurations.RequestPipeline.OnMessageCreating = args => new Microsoft.OData.Client.HttpClientRequestMessage(args);
    return ctx;
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
