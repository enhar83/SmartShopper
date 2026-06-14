using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.OrderDtos;
using FluentValidation;

namespace Business_Layer.Validators.CheckoutValidators
{
    public class CheckoutPaymentValidator : AbstractValidator<CheckoutPaymentDto>
    {
        public CheckoutPaymentValidator()
        {
            RuleFor(x => x.AddressId)
                .NotEmpty().WithMessage("Please select a delivery address for your order.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("The name on the card cannot be left blank.")
                .MinimumLength(2).WithMessage("The name must be at least two characters long.")
                .MaximumLength(50).WithMessage("The name cannot be longer than 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("The surname on the card cannot be left blank.")
                .MinimumLength(2).WithMessage("The surname must be at least two characters long.")
                .MaximumLength(50).WithMessage("The surname cannot be longer than 50 characters.");

            RuleFor(x => x.CardNumber)
                .NotEmpty().WithMessage("The card number cannot be left blank.")
                .Length(16).WithMessage("The card number must be exactly 16 digits long.")
                .Matches(@"^\d+$").WithMessage("The card number must consist only of digits.");

            RuleFor(x => x.Cvv)
                .NotEmpty().WithMessage("The CVV security code cannot be left blank.")
                .Length(3).WithMessage("The CVV code must be exactly 3 digits long.")
                .Matches(@"^\d+$").WithMessage("The CVV code must consist only of digits.");
        }
    }
}
