using Xunit;
using Moq;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WeatherApp.Api.Controllers;
using WeatherApp.Api.Application.Interfaces;
using WeatherApp.Shared.Results;
using WeatherApp.Shared.DTOs.Requests;
using WeatherApp.Shared.DTOs.Responses;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WeatherApp.Tests;

public class ControllersTests
{
    private readonly Mock<ILocationService> _mockLocationService;
    private readonly Mock<IWeatherService> _mockWeatherService;
    private readonly Mock<ILogger<CountriesController>> _mockCountriesLogger;
    private readonly Mock<ILogger<WeatherController>> _mockWeatherLogger;
    private readonly Mock<IValidator<GetCitiesRequest>> _mockCitiesValidator;
    private readonly Mock<IValidator<GetWeatherRequest>> _mockWeatherValidator;

    public ControllersTests()
    {
        _mockLocationService = new Mock<ILocationService>();
        _mockWeatherService = new Mock<IWeatherService>();
        _mockCountriesLogger = new Mock<ILogger<CountriesController>>();
        _mockWeatherLogger = new Mock<ILogger<WeatherController>>();
        _mockCitiesValidator = new Mock<IValidator<GetCitiesRequest>>();
        _mockWeatherValidator = new Mock<IValidator<GetWeatherRequest>>();
    }

    [Fact]
    public async Task CountriesController_GetCountries_ReturnsOkWithCountries()
    {
        // Arrange
        var countriesList = new List<CountryDto>
        {
            new CountryDto { Code = "US", Name = "United States" }
        };
        _mockLocationService.Setup(s => s.GetCountriesAsync())
            .ReturnsAsync(countriesList);

        var controller = new CountriesController(_mockLocationService.Object, _mockCountriesLogger.Object, _mockCitiesValidator.Object);

        // Act
        var result = await controller.GetCountries();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseResult = Assert.IsType<Result<List<CountryDto>>>(okResult.Value);
        Assert.True(responseResult.IsSuccess);
        Assert.Single(responseResult.Value!);
        Assert.Equal("US", responseResult.Value![0].Code);
    }

    [Fact]
    public async Task CountriesController_GetCities_Success_ReturnsOkWithCities()
    {
        // Arrange
        var request = new GetCitiesRequest { CountryCode = "US" };
        var citiesList = new List<CityDto>
        {
            new CityDto { Name = "New York", CountryCode = "US" }
        };

        _mockCitiesValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult()); // Success validation

        _mockLocationService.Setup(s => s.GetCitiesByCountryAsync("US"))
            .ReturnsAsync(citiesList);

        var controller = new CountriesController(_mockLocationService.Object, _mockCountriesLogger.Object, _mockCitiesValidator.Object);

        // Act
        var result = await controller.GetCities(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseResult = Assert.IsType<Result<List<CityDto>>>(okResult.Value);
        Assert.True(responseResult.IsSuccess);
        Assert.Single(responseResult.Value!);
        Assert.Equal("New York", responseResult.Value![0].Name);
    }

    [Fact]
    public async Task CountriesController_GetCities_ValidationError_ReturnsBadRequest()
    {
        // Arrange
        var request = new GetCitiesRequest { CountryCode = "USA" }; // invalid length
        var validationFailure = new ValidationFailure("CountryCode", "Country code must be 2 characters");
        
        _mockCitiesValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { validationFailure }));

        var controller = new CountriesController(_mockLocationService.Object, _mockCountriesLogger.Object, _mockCitiesValidator.Object);

        // Act
        var result = await controller.GetCities(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var responseResult = Assert.IsType<Result>(badRequestResult.Value);
        Assert.False(responseResult.IsSuccess);
        Assert.Equal("Validation failed", responseResult.ErrorMessage);
        Assert.Contains("Country code must be 2 characters", responseResult.Errors!);
    }

    [Fact]
    public async Task WeatherController_GetWeather_Success_ReturnsOkWithWeatherData()
    {
        // Arrange
        var request = new GetWeatherRequest { CityName = "Jakarta" };
        var weatherResponse = new WeatherResponseDto
        {
            Location = "Jakarta, Indonesia",
            TemperatureCelsius = 28.0
        };

        _mockWeatherValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockWeatherService.Setup(s => s.GetWeatherForCityAsync("Jakarta"))
            .ReturnsAsync(Result<WeatherResponseDto>.Success(weatherResponse));

        var controller = new WeatherController(_mockWeatherService.Object, _mockWeatherLogger.Object, _mockWeatherValidator.Object);

        // Act
        var result = await controller.GetWeather(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseResult = Assert.IsType<Result<WeatherResponseDto>>(okResult.Value);
        Assert.True(responseResult.IsSuccess);
        Assert.Equal("Jakarta, Indonesia", responseResult.Value!.Location);
        Assert.Equal(28.0, responseResult.Value!.TemperatureCelsius);
    }

    [Fact]
    public async Task WeatherController_GetWeather_ServiceError_ReturnsNotFound()
    {
        // Arrange
        var request = new GetWeatherRequest { CityName = "Gotham" };

        _mockWeatherValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockWeatherService.Setup(s => s.GetWeatherForCityAsync("Gotham"))
            .ReturnsAsync(Result<WeatherResponseDto>.Failure("City not found"));

        var controller = new WeatherController(_mockWeatherService.Object, _mockWeatherLogger.Object, _mockWeatherValidator.Object);

        // Act
        var result = await controller.GetWeather(request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var responseResult = Assert.IsType<Result>(notFoundResult.Value);
        Assert.False(responseResult.IsSuccess);
        Assert.Equal("City not found", responseResult.ErrorMessage);
    }

    [Fact]
    public async Task WeatherController_GetWeather_ValidationError_ReturnsBadRequest()
    {
        // Arrange
        var request = new GetWeatherRequest { CityName = "A" }; // invalid length
        var validationFailure = new ValidationFailure("CityName", "City name must be at least 2 characters long");

        _mockWeatherValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { validationFailure }));

        var controller = new WeatherController(_mockWeatherService.Object, _mockWeatherLogger.Object, _mockWeatherValidator.Object);

        // Act
        var result = await controller.GetWeather(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var responseResult = Assert.IsType<Result>(badRequestResult.Value);
        Assert.False(responseResult.IsSuccess);
        Assert.Equal("Validation failed", responseResult.ErrorMessage);
        Assert.Contains("City name must be at least 2 characters long", responseResult.Errors!);
    }
}
