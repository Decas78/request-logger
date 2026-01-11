using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design.Serialization;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Microsoft.VisualBasic;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

var app = builder.Build();

app.Urls.Add("http://0.0.0.0:8080");



// Middleware

app.Use(async (context, next) =>
{
    // Log basic request info (always safe)
    Console.WriteLine($"Incoming Request: {context.Request.Method} {context.Request.Path}{context.Request.QueryString}");

    var startTime = DateTime.UtcNow;

    string requestBody = string.Empty;
    long? originalPosition = null;

    // Only try to read body if it's a method that can have one AND there's content
    if (HttpMethods.IsPost(context.Request.Method) ||
        HttpMethods.IsPut(context.Request.Method) ||
        HttpMethods.IsPatch(context.Request.Method))
    {
        // Enable buffering so we can read and reset
        context.Request.EnableBuffering();

        if (context.Request.Body.CanRead && context.Request.ContentLength > 0)
        {
            // Remember position in case it's already been partially read (rare but safe)
            if (context.Request.Body.CanSeek)
            {
                originalPosition = context.Request.Body.Position;
            }

            using var reader = new StreamReader(
                context.Request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            requestBody = await reader.ReadToEndAsync();

            // Only reset position if the stream supports seeking
            if (context.Request.Body.CanSeek)
            {
                context.Request.Body.Position = 0;
            }
        }
    }

    // Continue processing the actual request
    await next();

    // Calculate latency after request processing
    var endTime = DateTime.UtcNow;
    var latencyMs = (endTime - startTime).TotalMilliseconds;

    // Build log entry
    var logEntry = new
    {
        method = context.Request.Method,
        path = context.Request.Path.Value,
        queryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null,
        bodySizeBytes = string.IsNullOrEmpty(requestBody) ? 0 : Encoding.UTF8.GetByteCount(requestBody),
        latencyMs = (int)latencyMs,
        responseStatusCode = context.Response.StatusCode,
        clientIp = context.Connection.RemoteIpAddress?.ToString(),
        timestamp = DateTime.UtcNow.ToString("o")
    };

    // Send to logging service
    var httpClient = context.RequestServices.GetRequiredService<IHttpClientFactory>().CreateClient();
    try
    {
        var json = System.Text.Json.JsonSerializer.Serialize(logEntry);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        await httpClient.PostAsync("http://processor:8000/logsimple", content);
    }
    catch
    {
        // Ignore failures in logging to avoid breaking main request
    }
});



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Controllers



// Test Endpoints
app.MapGet("/", () => Results.Ok(new {message = "Request Logger API is running."}));

app.MapGet("/health", () => new { status = "healthyyeah" });

app.MapPost("/testwrite", (PostTest input) =>
{
    // The input parameter is automatically deserialized from JSON
    return Results.Ok(new { 
        message = "Received POST with JSON data", 
        receivedData = input,
        validation = $"Title: {input.Title}, Content: {input.Content}, Id: {input.Id}"
    });
});

app.MapPost("/testinvalid", (PostTestNR input) =>
{
    var errors = new List<string>();
    if (string.IsNullOrEmpty(input.Title))
    {
        errors.Add("Title is required");
    }
    if (string.IsNullOrEmpty(input.Content))
    {
        errors.Add("Content is required");
    }
    if (input.Id <= 0)
    {
        errors.Add("Id is required and must be non-zero");
    }
    if (errors.Any())
    {
        return Results.BadRequest(new { errors });
    }
    return Results.Ok(new { 
        message = "Received POST with JSON data", 
        receivedData = input,
        validation = $"Title: {input.Title}, Content: {input.Content}, Id: {input.Id}"
    });
});

app.MapGet("/testreadslow", async () =>
{
    await Task.Delay(5000); // Simulate accessing a slow resource such as a database
    var sampleData = new PostTest
    {
        Title = "Sample Title",
        Content = "This is some sample content.",
        Id = 1
    };
    return Results.Ok(sampleData);
});




app.Run();
public partial class Program { }
