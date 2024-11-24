using FluentValidation;

namespace DaprizzaWeb.Models.Validators;

public class PizzaValidator : AbstractValidator<Pizza>
{
    public PizzaValidator()
    {
        RuleFor(p => p.Size)
            .NotEmpty()
            .IsInEnum();
        RuleFor(p => p.Toppings)
            .NotEmpty();
        RuleFor(p => p.Toppings)
            .Must(toppings => toppings.All(t => !string.IsNullOrWhiteSpace(t) && t.Length <= 255))
            .WithMessage("Invalid topping found.");
    }
}
