using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api.Tests;

public class PaginationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public PaginationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("ApiKey", "asd123");
    }

    [Theory]
    [InlineData("Orders", 100)]
    [InlineData("Customers", 110)]
    public async Task EntitySet_Should_Return_Configured_Page_Size(string entitySet, int expectedMaxPageSize)
    {
        var response = await _client.GetAsync($"/{entitySet}");

        response.IsSuccessStatusCode.Should().BeTrue();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ODataResponse<EntityBase>>(content, _jsonOptions);
        result!.Value.Count.Should().BeLessThanOrEqualTo(expectedMaxPageSize);
    }

    [Theory]
    [InlineData("Orders", 10)]
    [InlineData("Customers", 15)]
    public async Task EntitySet_Should_Respect_Top_Parameter(string entitySet, int topValue)
    {
        var response = await _client.GetAsync($"/{entitySet}?$top={topValue}");

        response.IsSuccessStatusCode.Should().BeTrue();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ODataResponse<EntityBase>>(content, _jsonOptions)!;
        result.Value.Count.Should().Be(topValue);
    }

    [Theory]
    [InlineData("Orders", 101)]
    [InlineData("Customers", 111)]
    public async Task EntitySet_Should_Enforce_Maximum_Top_Limit(string entitySet, int topValue)
    {
        var response = await _client.GetAsync($"/{entitySet}?$top={topValue}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.ToLower().Should().Contain("limit");
    }

    [Theory]
    [InlineData("Orders")]
    [InlineData("Customers")]
    public async Task EntitySet_Should_Support_Server_Driven_Pagination(string entitySet)
    {
        var firstResponse = await _client.GetAsync($"/{entitySet}");
        firstResponse.IsSuccessStatusCode.Should().BeTrue();
        var firstContent = await firstResponse.Content.ReadAsStringAsync();
        var firstResult = JsonSerializer.Deserialize<ODataNextLinkResponse<EntityBase>>(firstContent, _jsonOptions)!;

        firstResult.NextLink.Should().NotBeNullOrEmpty();
        firstResult.NextLink.Should().Contain("$skiptoken=");

        var secondResponse = await _client.GetAsync(new Uri(firstResult.NextLink).PathAndQuery);
        secondResponse.IsSuccessStatusCode.Should().BeTrue();
        var secondContent = await secondResponse.Content.ReadAsStringAsync();
        var secondResult = JsonSerializer.Deserialize<ODataResponse<EntityBase>>(secondContent, _jsonOptions)!;

        var firstPageIds = firstResult.Value.Select(x => x.Id).ToList();
        var secondPageIds = secondResult.Value.Select(x => x.Id).ToList();
        firstPageIds.Should().NotIntersectWith(secondPageIds);
    }
}

public class EntityBase
{
    public int Id { get; set; }
}

public class ODataResponse<T>
{
    public List<T> Value { get; set; } = new();
}

public class ODataNextLinkResponse<T> : ODataResponse<T>
{
    [JsonPropertyName("@odata.nextLink")]
    public string? NextLink { get; set; }
}
