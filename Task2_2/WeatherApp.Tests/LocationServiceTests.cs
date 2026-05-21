using Xunit;
using Microsoft.EntityFrameworkCore;
using WeatherApp.Api.Infrastructure.Data;
using WeatherApp.Api.Infrastructure.Services;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace WeatherApp.Tests;

public class LocationServiceTests
{
    private readonly WeatherDbContext _context;
    private readonly LocationService _locationService;

    public LocationServiceTests()
    {
        var options = new DbContextOptionsBuilder<WeatherDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new WeatherDbContext(options);
        // Ensure Database is created and seeded (EnsureCreated triggers seed method in DbContext)
        _context.Database.EnsureCreated();

        _locationService = new LocationService(_context);
    }

    [Fact]
    public async Task GetCountriesAsync_ReturnsSeededCountries()
    {
        // Act
        var countries = await _locationService.GetCountriesAsync();

        // Assert
        Assert.NotNull(countries);
        Assert.NotEmpty(countries);
        Assert.Contains(countries, c => c.Code == "US");
        Assert.Contains(countries, c => c.Code == "ID");
    }

    [Fact]
    public async Task GetCitiesByCountryAsync_WithValidCountryCode_ReturnsCorrectCities()
    {
        // Act
        var cities = await _locationService.GetCitiesByCountryAsync("ID");

        // Assert
        Assert.NotNull(cities);
        Assert.NotEmpty(cities);
        Assert.All(cities, c => Assert.Equal("ID", c.CountryCode));
        Assert.Contains(cities, c => c.Name == "Jakarta");
    }

    [Fact]
    public async Task GetCitiesByCountryAsync_WithInvalidCountryCode_ReturnsEmptyList()
    {
        // Act
        var cities = await _locationService.GetCitiesByCountryAsync("XX");

        // Assert
        Assert.NotNull(cities);
        Assert.Empty(cities);
    }
}
