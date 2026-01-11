using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetRootEndpoint_ReturnsOkAndCorrectMessage()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"message\":\"Request Logger API is running.\"", responseString);
    }

    [Fact]
    public async Task GetHealthEndpoint_ReturnsHealthyStatus()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"status\":\"healthyyeah\"", responseString);
    }

    [Fact]
    public async Task PostTestWriteEndpoint_ReturnsOkWithData()
    {
        // Arrange
        var testData = new PostTest { Title = "Test Title", Content = "Test Content", Id = 123 };
        var json = JsonSerializer.Serialize(testData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/testwrite", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"message\":\"Received POST with JSON data\"", responseString);
        Assert.Contains("\"title\":\"Test Title\"", responseString);
    }

    [Fact]
    public async Task PostTestInvalidEndpoint_WithValidData_ReturnsOk()
    {
        // Arrange
        var testData = new PostTestNR { Title = "Valid Title", Content = "Valid Content", Id = 456 };
        var json = JsonSerializer.Serialize(testData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/testinvalid", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"message\":\"Received POST with JSON data\"", responseString);
    }

    [Fact]
    public async Task PostTestInvalidEndpoint_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var testData = new PostTestNR { Title = "", Content = "", Id = 0 };
        var json = JsonSerializer.Serialize(testData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/testinvalid", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"errors\"", responseString);
    }

    [Fact]
    public async Task GetTestReadSlowEndpoint_ReturnsDataAfterDelay()
    {
        // Act
        var response = await _client.GetAsync("/testreadslow");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"title\":\"Sample Title\"", responseString);
        Assert.Contains("\"content\":\"This is some sample content.\"", responseString);
        Assert.Contains("\"id\":1", responseString);
    }
}
