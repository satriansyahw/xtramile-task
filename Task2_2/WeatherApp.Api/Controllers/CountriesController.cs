using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using WeatherApp.Shared.Results;
using WeatherApp.Shared.DTOs.Requests;
using WeatherApp.Shared.DTOs.Responses;
using WeatherApp.Api.Application.Interfaces;

namespace WeatherApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CountriesController : ControllerBase
{
    private readonly ILocationService _locationService;
    private readonly ILogger<CountriesController> _logger;
    private readonly IValidator<GetCitiesRequest> _validator;

    public CountriesController(ILocationService locationService, ILogger<CountriesController> logger, IValidator<GetCitiesRequest> validator)
    {
        _locationService = locationService;
        _logger = logger;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> GetCountries()
    {
        _logger.LogInformation("Fetching all countries");
        var countries = await _locationService.GetCountriesAsync();
        _logger.LogInformation("Successfully fetched {Count} countries", countries.Count);
        return Ok(Result<List<CountryDto>>.Success(countries));
    }

    [HttpGet("{CountryCode}/cities")]
    public async Task<IActionResult> GetCities([FromRoute] GetCitiesRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(Result.Failure("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage)));
        }

        _logger.LogInformation("Fetching cities for country code: {CountryCode}", request.CountryCode);
        var cities = await _locationService.GetCitiesByCountryAsync(request.CountryCode);
        _logger.LogInformation("Successfully fetched {Count} cities for {CountryCode}", cities.Count, request.CountryCode);
        return Ok(Result<List<CityDto>>.Success(cities));
    }
}
