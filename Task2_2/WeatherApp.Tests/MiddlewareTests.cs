using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WeatherApp.Api.Infrastructure.Middleware;
using WeatherApp.Shared.Results;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace WeatherApp.Tests;

public class MiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionMiddleware>> _mockLogger;
    private readonly GlobalExceptionMiddleware _middleware;

    public MiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<GlobalExceptionMiddleware>>();
        _middleware = new GlobalExceptionMiddleware(_mockLogger.Object);
    }

    [Fact]
    public async Task InvokeAsync_NoError_CallsNextDelegate()
    {
        // Arrange
        var context = new DefaultHttpContext();
        bool nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        // Act
        await _middleware.InvokeAsync(context, next);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(200, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_OnException_LogsAndReturnsErrorResult()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        var expectedException = new InvalidOperationException("Test exception");
        RequestDelegate next = (ctx) => throw expectedException;

        // Act
        await _middleware.InvokeAsync(context, next);

        // Assert
        // Check Status Code
        Assert.Equal(500, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);

        // Verify Logger
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An unexpected error occurred")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify response content
        responseStream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(responseStream);
        string responseBody = await reader.ReadToEndAsync();
        
        var result = JsonSerializer.Deserialize<Result>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal("An unexpected server error occurred. Please try again later.", result.ErrorMessage);
    }
}
