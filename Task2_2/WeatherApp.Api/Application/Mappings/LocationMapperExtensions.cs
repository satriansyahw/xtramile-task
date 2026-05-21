using WeatherApp.Shared.DTOs.Responses;
using WeatherApp.Api.Domain.Models;
using System.Collections.Generic;
using System.Linq;

namespace WeatherApp.Api.Application.Mappings;

public static class LocationMapperExtensions
{
    public static CountryDto ToDto(this Country country)
    {
        return new CountryDto
        {
            Code = country.Code,
            Name = country.Name
        };
    }

    public static List<CountryDto> ToDtoList(this IEnumerable<Country> countries)
    {
        return countries.Select(c => c.ToDto()).ToList();
    }

    public static CityDto ToDto(this City city)
    {
        return new CityDto
        {
            Name = city.Name,
            CountryCode = city.CountryCode
        };
    }

    public static List<CityDto> ToDtoList(this IEnumerable<City> cities)
    {
        return cities.Select(c => c.ToDto()).ToList();
    }
}
