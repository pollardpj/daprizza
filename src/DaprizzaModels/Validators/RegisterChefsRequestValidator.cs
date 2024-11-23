using FluentValidation;

namespace DaprizzaModels.Validators;

public class RegisterChefsRequestValidator : AbstractValidator<RegisterChefsRequest>
{
    public RegisterChefsRequestValidator()
    {
        RuleFor(x => x.Chefs)
            .NotEmpty();
        RuleFor(x => x.Chefs)
            .Must(chefs => chefs.All(c => !string.IsNullOrWhiteSpace(c.Name) && c.Name.Length <= 255))
            .WithMessage("Invalid Chef found.")
            .Must(chefs =>
                chefs.Select(c => c.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() == chefs.Count())
            .WithMessage("Duplicate Chefs found.");
    }
}
