namespace WeatherApp.Shared.DTOs.Responses;

public class WeatherResponseDto
{
    public string Location { get; set; } = string.Empty;
    public string TimeUTC { get; set; } = string.Empty;
    public string Wind { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public string SkyConditions { get; set; } = string.Empty;
    public double TemperatureFahrenheit { get; set; }
    public double TemperatureCelsius { get; set; }
    public double DewPoint { get; set; }
    public int RelativeHumidity { get; set; }
    public double Pressure { get; set; }
}
