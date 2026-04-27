using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.AuthDtos;
using FluentValidation;

namespace Business_Layer.Validators.AuthValidators
{
    public class ConfirmUserEmailValidator : AbstractValidator<ConfirmUserEmailDto>
    {
        public ConfirmUserEmailValidator()
        {
            RuleFor(x => x.ActivationCode)
                .NotEmpty().WithMessage("Activation code is required.")
                .Length(6).WithMessage("Activation code must be exactly 6 digits.")
                .Matches(@"^\d{6}$").WithMessage("Activation code must contain only numbers.");
        }
    }
}
