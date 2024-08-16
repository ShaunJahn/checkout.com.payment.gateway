using FluentValidation;

namespace PaymentGateway.Api.Application.Commands
{
    public class PaymentRequestValidator : AbstractValidator<PaymentRequestCommand>
    {
        public PaymentRequestValidator()
        {
            RuleFor(x => x.CardNumber)
                .NotEmpty().WithMessage("Card number is required")
                .Length(14, 19).WithMessage("Card number must be between 14 and 19 characters long")
                .Matches("^[0-9]+$").WithMessage("Card number must contain only numeric characters");

            RuleFor(x => x.ExpiryMonth)
                .NotEmpty().WithMessage("Expiry month is required")
                .InclusiveBetween(1, 12).WithMessage("Expiry month must be between 1 and 12");

            RuleFor(x => x.ExpiryYear)
                .NotEmpty().WithMessage("Expiry year is required")
                .GreaterThanOrEqualTo(2024).WithMessage("Expiry year must be in the future");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required")
                .Length(3).WithMessage("Currency code must be 3 characters long")
                .Matches("^(USD|EUR|GBP)$").WithMessage("Currency must be USD, EUR, or GBP");

            RuleFor(x => x.Amount)
                .NotEmpty().WithMessage("Amount is required")
                .GreaterThan(0).WithMessage("Amount must be greater than zero");

            RuleFor(x => x.Cvv)
                .NotEmpty().WithMessage("CVV is required")
                .Length(3, 4).WithMessage("CVV must be 3 or 4 digits long")
                .Matches("^[0-9]{3,4}$").WithMessage("CVV must contain only numeric characters and be 3 or 4 digits");
        }
    }
}
