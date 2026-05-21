using WeatherApp.Shared.Results;
using WeatherApp.Shared.DTOs.Responses;

namespace WeatherApp.Api.Application.Interfaces;

public interface IWeatherService
{
    Task<Result<WeatherResponseDto>> GetWeatherForCityAsync(string cityName);
    double ConvertFahrenheitToCelsius(double fahrenheit);
}
