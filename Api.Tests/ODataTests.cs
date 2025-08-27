using Microsoft.AspNetCore.Mvc.Testing;

namespace Api.Tests;

public class ODataTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ODataTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ServiceDocument_Is_Verified()
    {
        var response = await _client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        await VerifyJson(json);
    }

    [Fact]
    public async Task Metadata_Is_Verified()
    {
        var response = await _client.GetAsync("/$metadata");
        response.EnsureSuccessStatusCode();
        var xml = await response.Content.ReadAsStringAsync();
        await VerifyXml(xml);
    }
}
