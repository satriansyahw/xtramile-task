using Xunit;
using WeatherApp.Api.Application.Validators;
using WeatherApp.Shared.DTOs.Requests;

namespace WeatherApp.Tests;

public class ValidatorTests
{
    private readonly GetCitiesRequestValidator _citiesValidator;
    private readonly GetWeatherRequestValidator _weatherValidator;

    public ValidatorTests()
    {
        _citiesValidator = new GetCitiesRequestValidator();
        _weatherValidator = new GetWeatherRequestValidator();
    }

    [Theory]
    [InlineData("US", true)]
    [InlineData("ID", true)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("USA", false)]
    [InlineData("U", false)]
    [InlineData(null, false)]
    public void GetCitiesRequestValidator_ValidationTests(string? countryCode, bool expectedIsValid)
    {
        // Arrange
        var request = new GetCitiesRequest { CountryCode = countryCode! };

        // Act
        var result = _citiesValidator.Validate(request);

        // Assert
        Assert.Equal(expectedIsValid, result.IsValid);
        if (!expectedIsValid)
        {
            Assert.NotEmpty(result.Errors);
        }
    }

    [Theory]
    [InlineData("Jakarta", true)]
    [InlineData("NY", true)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("A", false)] // under 2 chars
    [InlineData(null, false)]
    public void GetWeatherRequestValidator_ValidationTests(string? cityName, bool expectedIsValid)
    {
        // Arrange
        var request = new GetWeatherRequest { CityName = cityName! };

        // Act
        var result = _weatherValidator.Validate(request);

        // Assert
        Assert.Equal(expectedIsValid, result.IsValid);
        if (!expectedIsValid)
        {
            Assert.NotEmpty(result.Errors);
        }
    }

    [Fact]
    public void GetWeatherRequestValidator_TooLongCityName_FailsValidation()
    {
        // Arrange
        var longCityName = new string('A', 101);
        var request = new GetWeatherRequest { CityName = longCityName };

        // Act
        var result = _weatherValidator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetWeatherRequest.CityName));
    }
}
