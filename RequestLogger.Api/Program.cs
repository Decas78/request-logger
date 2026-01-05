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

    // Build log entry
    var logEntry = new
    {
        method = context.Request.Method,
        path = context.Request.Path.Value,
        query = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null,
        bodySizeBytes = string.IsNullOrEmpty(requestBody) ? 0 : Encoding.UTF8.GetByteCount(requestBody),
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

    // Continue processing the actual request
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Endpoints
app.MapGet("/health", () => Results.Ok(new { status = "healthyyeah" }));

app.Run();