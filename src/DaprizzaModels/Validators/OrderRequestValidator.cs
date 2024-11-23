using FluentValidation;

namespace DaprizzaModels.Validators;

public class OrderRequestValidator : AbstractValidator<OrderRequest>
{
    public OrderRequestValidator()
    {
        RuleFor(x => x.Pizzas)
            .NotEmpty();
        RuleForEach(x => x.Pizzas)
            .SetValidator(new PizzaValidator());
        RuleFor(x => x.Address)
            .NotNull();
        RuleFor(x => x.Address)
            .SetValidator(new AddressValidator());
    }
}

