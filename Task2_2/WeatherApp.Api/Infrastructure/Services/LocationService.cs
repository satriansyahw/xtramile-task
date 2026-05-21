using Microsoft.EntityFrameworkCore;
using WeatherApp.Api.Domain.Models;
using WeatherApp.Api.Infrastructure.Data;
using WeatherApp.Api.Application.Interfaces;
using WeatherApp.Shared.DTOs.Responses;
using WeatherApp.Api.Application.Mappings;

namespace WeatherApp.Api.Infrastructure.Services;

public class LocationService : ILocationService
{
    private readonly WeatherDbContext _context;

    public LocationService(WeatherDbContext context)
    {
        _context = context;
        // Ensure database is created and seeded
        _context.Database.EnsureCreated();
    }

    public async Task<List<CountryDto>> GetCountriesAsync()
    {
        var countries = await _context.Countries.ToListAsync();
        return countries.ToDtoList();
    }

    public async Task<List<CityDto>> GetCitiesByCountryAsync(string countryCode)
    {
        var cities = await _context.Cities
            .Where(c => c.CountryCode == countryCode)
            .ToListAsync();
        
        return cities.ToDtoList();
    }
}
