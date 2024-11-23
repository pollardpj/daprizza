using DaprizzaModels;
using FluentValidation;

namespace DaprizzaStore.Validators
{
    public class PizzaValidator : AbstractValidator<Pizza>
    {
        public PizzaValidator()
        {
            RuleFor(x => x.Size)
                .NotEmpty();
            RuleFor(x => x.Toppings)
                .NotEmpty();
            RuleFor(x => x.Toppings)
                .Must(t => t.All(t2 => !string.IsNullOrWhiteSpace(t2) && t2.Length <= 255))
                .WithMessage("Invalid topping found.");
        }
    }
}
