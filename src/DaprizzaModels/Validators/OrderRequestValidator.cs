using FluentValidation;

namespace DaprizzaModels.Validators;

public class OrderRequestValidator : AbstractValidator<OrderRequest>
{
    public OrderRequestValidator()
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

