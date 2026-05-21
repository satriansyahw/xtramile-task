using FluentValidation;
using WeatherApp.Shared.DTOs.Requests;

namespace WeatherApp.Api.Application.Validators;

public class GetCitiesRequestValidator : AbstractValidator<GetCitiesRequest>
{
    public GetCitiesRequestValidator()
    {
        RuleFor(x => x.CountryCode)
            .NotEmpty().WithMessage("Country code is required.")
            .Length(2).WithMessage("Country code must be exactly 2 characters (e.g., 'ID', 'US').");
    }
}
