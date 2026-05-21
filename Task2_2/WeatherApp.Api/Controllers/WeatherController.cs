using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using WeatherApp.Shared.Results;
using WeatherApp.Shared.DTOs.Requests;
using WeatherApp.Api.Application.Interfaces;

namespace WeatherApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;
    private readonly ILogger<WeatherController> _logger;
    private readonly IValidator<GetWeatherRequest> _validator;

    public WeatherController(IWeatherService weatherService, ILogger<WeatherController> logger, IValidator<GetWeatherRequest> validator)
    {
        _weatherService = weatherService;
        _logger = logger;
        _validator = validator;
    }

    [HttpGet("{CityName}")]
    public async Task<IActionResult> GetWeather([FromRoute] GetWeatherRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(Result.Failure("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage)));
        }

        _logger.LogInformation("Fetching weather data for city: {CityName}", request.CityName);
        var result = await _weatherService.GetWeatherForCityAsync(request.CityName);
        
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Weather data not found for city: {CityName}. Reason: {ErrorMessage}", request.CityName, result.ErrorMessage);
            return NotFound(Result.Failure(result.ErrorMessage));
        }

        _logger.LogInformation("Successfully fetched weather data for city: {CityName}", request.CityName);
        return Ok(result);
    }
}
