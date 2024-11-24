using FluentValidation;

namespace DaprizzaModels.Validators;

public class RegisterDriversRequestValidator : AbstractValidator<RegisterDriversRequest>
{
    public RegisterDriversRequestValidator()
    {
        RuleFor(x => x.Drivers)
            .NotEmpty();
        RuleFor(x => x.Drivers)
            .Must(chefs => chefs.All(c => !string.IsNullOrWhiteSpace(c.Name) && c.Name.Length <= 255))
            .WithMessage("Invalid Driver found.")
            .Must(chefs =>
                chefs.Select(c => c.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() == chefs.Count())
            .WithMessage("Duplicate Driver found.");
    }
}
