using DaprizzaModels;
using FluentValidation;

namespace DaprizzaStore.Validators;

public class AddressValidator : AbstractValidator<Address>
{
    public AddressValidator()
    {
        RuleFor(x => x.Postcode)
            .NotEmpty()
            .MaximumLength(255);
        RuleFor(x => x.HouseNumberOrName)
            .NotEmpty()
            .MaximumLength(255);
    }
}
