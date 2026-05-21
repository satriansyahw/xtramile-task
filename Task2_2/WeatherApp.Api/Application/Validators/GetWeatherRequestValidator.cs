using FluentValidation;
using WeatherApp.Shared.DTOs.Requests;

namespace WeatherApp.Api.Application.Validators;

public class GetWeatherRequestValidator : AbstractValidator<GetWeatherRequest>
{
    public GetWeatherRequestValidator()
    {
        RuleFor(x => x.CityName)
            .NotEmpty().WithMessage("City name is required.")
            .MinimumLength(2).WithMessage("City name must be at least 2 characters long.")
            .MaximumLength(100).WithMessage("City name must not exceed 100 characters.");
    }
}
