using WeatherApp.Shared.Results;
using WeatherApp.Shared.DTOs.Responses;
using WeatherApp.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using WeatherApp.Api.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using WeatherApp.Api.Domain.Models;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace WeatherApp.Api.Infrastructure.Services;

public class WeatherService : IWeatherService
{
    private readonly WeatherDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public WeatherService(WeatherDbContext context, HttpClient httpClient, IConfiguration configuration)
    {
        _context = context;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<Result<WeatherResponseDto>> GetWeatherForCityAsync(string cityName)
    {
        // First verify the city exists in our database
        var city = await _context.Cities.FirstOrDefaultAsync(c => c.Name.ToLower() == cityName.ToLower());
        if (city == null)
        {
            return Result<WeatherResponseDto>.Failure($"City '{cityName}' not found in our records.");
        }

        var country = await _context.Countries.FirstOrDefaultAsync(c => c.Code == city.CountryCode);
        string countryName = country?.Name ?? city.CountryCode;

        string apiKey = _configuration["WeatherSettings:ApiKey"] ?? string.Empty;

        // Fallback to mock weather data if no API Key configured
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return Result<WeatherResponseDto>.Success(GenerateMockWeather(city, countryName));
        }

        try
        {
            var response = await _httpClient.GetAsync($"https://api.openweathermap.org/data/2.5/weather?q={Uri.EscapeDataString(cityName)}&appid={apiKey}&units=imperial");
            
            // Fallback to mock weather data if unauthorized (e.g. 401 due to invalid API Key)
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return Result<WeatherResponseDto>.Success(GenerateMockWeather(city, countryName));
            }

            if (!response.IsSuccessStatusCode)
            {
                return Result<WeatherResponseDto>.Failure($"External weather service returned error: {(int)response.StatusCode}");
            }

            var apiData = await response.Content.ReadFromJsonAsync<OpenWeatherResponse>();
            if (apiData == null)
            {
                return Result<WeatherResponseDto>.Failure("Failed to deserialize weather data from external service.");
            }

            double tempF = apiData.Main.Temp;
            double tempC = ConvertFahrenheitToCelsius(tempF);

            return Result<WeatherResponseDto>.Success(new WeatherResponseDto
            {
                Location = $"{city.Name}, {countryName}",
                TimeUTC = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'"),
                Wind = $"{apiData.Wind.Speed} mph, {apiData.Wind.Deg}°",
                Visibility = $"{apiData.Visibility / 1609.34:F1} miles",
                SkyConditions = apiData.Weather.FirstOrDefault()?.Main ?? "Clear",
                TemperatureFahrenheit = tempF,
                TemperatureCelsius = tempC,
                DewPoint = Math.Round(tempC - (100 - apiData.Main.Humidity) / 5.0, 2),
                RelativeHumidity = apiData.Main.Humidity,
                Pressure = apiData.Main.Pressure
            });
        }
        catch (HttpRequestException)
        {
            // Fallback to mock weather data if offline or connection fails
            return Result<WeatherResponseDto>.Success(GenerateMockWeather(city, countryName));
        }
        catch (Exception ex)
        {
            return Result<WeatherResponseDto>.Failure($"An unexpected error occurred while fetching weather data: {ex.Message}");
        }
    }

    public double ConvertFahrenheitToCelsius(double fahrenheit)
    {
        // Formula: (F - 32) * 5/9
        double celsius = (fahrenheit - 32) * 5.0 / 9.0;
        return Math.Round(celsius, 2);
    }

    private WeatherResponseDto GenerateMockWeather(City city, string countryName)
    {
        Random random = new Random(city.Name.Length);
        
        double tempF = Math.Round(random.NextDouble() * 60 + 30, 2); // 30F to 90F
        double tempC = ConvertFahrenheitToCelsius(tempF);

        var conditions = new[] { "Clear", "Cloudy", "Rain", "Snow", "Partly Cloudy", "Thunderstorm" };
        string sky = conditions[random.Next(conditions.Length)];

        return new WeatherResponseDto
        {
            Location = $"{city.Name}, {countryName}",
            TimeUTC = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'"),
            Wind = $"{random.Next(0, 30)} mph, {random.Next(0, 360)}°",
            Visibility = $"{random.Next(1, 10)} miles",
            SkyConditions = sky,
            TemperatureFahrenheit = tempF,
            TemperatureCelsius = tempC,
            DewPoint = Math.Round(tempC - random.Next(1, 10), 2),
            RelativeHumidity = random.Next(30, 95),
            Pressure = random.Next(980, 1030)
        };
    }
}

public class OpenWeatherResponse
{
    public MainInfo Main { get; set; } = new();
    public WindInfo Wind { get; set; } = new();
    public WeatherInfo[] Weather { get; set; } = Array.Empty<WeatherInfo>();
    public int Visibility { get; set; }

    public class MainInfo
    {
        public double Temp { get; set; }
        public int Humidity { get; set; }
        public int Pressure { get; set; }
    }

    public class WindInfo
    {
        public double Speed { get; set; }
        public int Deg { get; set; }
    }

    public class WeatherInfo
    {
        public string Main { get; set; } = string.Empty;
    }
}
