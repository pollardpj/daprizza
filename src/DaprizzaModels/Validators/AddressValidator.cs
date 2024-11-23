using FluentValidation;

namespace DaprizzaModels.Validators;

public class AddressValidator : AbstractValidator<Address>
{
    public AddressValidator()
    {
        RuleFor(a => a.Postcode)
            .NotEmpty()
            .MaximumLength(255);
        RuleFor(a => a.HouseNumberOrName)
            .NotEmpty()
            .MaximumLength(255);
    }
}
