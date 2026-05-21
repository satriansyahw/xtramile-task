using Xunit;
using Moq;
using Moq.Protected;
using Microsoft.EntityFrameworkCore;
using WeatherApp.Api.Infrastructure.Data;
using WeatherApp.Api.Infrastructure.Services;
using WeatherApp.Shared.Results;
using WeatherApp.Shared.DTOs.Responses;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;


namespace WeatherApp.Tests;

public class WeatherServiceTests
{
    private readonly WeatherDbContext _dbContext;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly Mock<IConfiguration> _mockConfiguration;

    public WeatherServiceTests()
    {
        var options = new DbContextOptionsBuilder<WeatherDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _dbContext = new WeatherDbContext(options);
        _dbContext.Database.EnsureCreated();

        _mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);

        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(c => c["WeatherSettings:ApiKey"]).Returns("test-api-key");
    }

    [Fact]
    public void ConvertFahrenheitToCelsius_CorrectlyConvertsFreezingPoint()
    {
        var service = new WeatherService(_dbContext, _httpClient, _mockConfiguration.Object);
        double celsius = service.ConvertFahrenheitToCelsius(32.0);
        Assert.Equal(0.0, celsius);
    }

    [Fact]
    public void ConvertFahrenheitToCelsius_CorrectlyConvertsBoilingPoint()
    {
        var service = new WeatherService(_dbContext, _httpClient, _mockConfiguration.Object);
        double celsius = service.ConvertFahrenheitToCelsius(212.0);
        Assert.Equal(100.0, celsius);
    }

    [Fact]
    public void ConvertFahrenheitToCelsius_CorrectlyConvertsBodyTemp()
    {
        var service = new WeatherService(_dbContext, _httpClient, _mockConfiguration.Object);
        double celsius = service.ConvertFahrenheitToCelsius(98.6);
        Assert.Equal(37.0, celsius);
    }

    [Fact]
    public async Task GetWeatherForCityAsync_ReturnsFailureForUnknownCity()
    {
        // Arrange
        var service = new WeatherService(_dbContext, _httpClient, _mockConfiguration.Object);

        // Act
        var result = await service.GetWeatherForCityAsync("Gotham");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal("City 'Gotham' not found in our records.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetWeatherForCityAsync_SuccessfulHttpCall_ReturnsWeatherResponse()
    {
        // Arrange
        var mockResponse = new OpenWeatherResponse
        {
            Main = new OpenWeatherResponse.MainInfo
            {
                Temp = 77.0,
                Humidity = 60,
                Pressure = 1013
            },
            Wind = new OpenWeatherResponse.WindInfo
            {
                Speed = 12.5,
                Deg = 180
            },
            Weather = new[]
            {
                new OpenWeatherResponse.WeatherInfo { Main = "Cloudy" }
            },
            Visibility = 10000 // 10km in meters
        };

        var json = JsonSerializer.Serialize(mockResponse);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri!.ToString().Contains("Jakarta")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            });

        var service = new WeatherService(_dbContext, _httpClient, _mockConfiguration.Object);

        // Act
        var result = await service.GetWeatherForCityAsync("Jakarta");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Jakarta, Indonesia", result.Value.Location);
        Assert.Equal(77.0, result.Value.TemperatureFahrenheit);
        Assert.Equal(25.0, result.Value.TemperatureCelsius); // (77 - 32) * 5/9 = 25
        Assert.Equal("Cloudy", result.Value.SkyConditions);
        Assert.Equal("12.5 mph, 180°", result.Value.Wind);
        Assert.Equal("6.2 miles", result.Value.Visibility); // 10000 / 1609.34 = 6.2 miles
    }

    [Fact]
    public async Task GetWeatherForCityAsync_FailedHttpCall_ReturnsFailure()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        var service = new WeatherService(_dbContext, _httpClient, _mockConfiguration.Object);

        // Act
        var result = await service.GetWeatherForCityAsync("Jakarta");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains("External weather service returned error: 500", result.ErrorMessage);
    }

    [Fact]
    public async Task GetWeatherForCityAsync_NoApiKey_ReturnsMockWeatherResponse()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["WeatherSettings:ApiKey"]).Returns((string?)null);
        var service = new WeatherService(_dbContext, _httpClient, _mockConfiguration.Object);

        // Act
        var result = await service.GetWeatherForCityAsync("Jakarta");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Jakarta, Indonesia", result.Value.Location);
        Assert.NotEmpty(result.Value.SkyConditions);
    }

    [Fact]
    public async Task GetWeatherForCityAsync_Http401Unauthorized_ReturnsMockWeatherResponse()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized
            });

        var service = new WeatherService(_dbContext, _httpClient, _mockConfiguration.Object);

        // Act
        var result = await service.GetWeatherForCityAsync("Jakarta");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Jakarta, Indonesia", result.Value.Location);
    }

    [Fact]
    public async Task GetWeatherForCityAsync_HttpException_ReturnsMockWeatherResponse()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Connection error"));

        var service = new WeatherService(_dbContext, _httpClient, _mockConfiguration.Object);

        // Act
        var result = await service.GetWeatherForCityAsync("Jakarta");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Jakarta, Indonesia", result.Value.Location);
    }
}
