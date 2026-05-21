using WeatherApp.Shared.DTOs.Responses;

namespace WeatherApp.Api.Application.Interfaces;

public interface ILocationService
{
    Task<List<CountryDto>> GetCountriesAsync();
    Task<List<CityDto>> GetCitiesByCountryAsync(string countryCode);
}
