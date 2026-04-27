using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.AuthDtos;
using FluentValidation;

namespace Business_Layer.Validators.AuthValidators
{
    public class LoginValidator:AbstractValidator<LoginDto>
    {
        public LoginValidator() 
        {
            RuleFor(x => x.UsernameOrEmail)
            .NotEmpty().WithMessage("Username or Email is required.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");
        }
    }
}
