using FluentValidation;

namespace DaprizzaWeb.Models.Validators;

public class OrderValidator : AbstractValidator<Order>
{
    public OrderValidator()
    {
        RuleFor(r => r.Pizzas)
            .NotEmpty();
        RuleForEach(r => r.Pizzas)
            .SetValidator(new PizzaValidator());
        RuleFor(r => r.Address)
            .NotNull();
        RuleFor(r => r.Address)
            .SetValidator(new AddressValidator());
    }
}

