using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.UserAddressDtos;
using FluentValidation;

namespace Business_Layer.Validators.UserAddressValidators
{
    public class UpdateUserAddressValidator:AbstractValidator<UpdateUserAddressDto>
    {
        public UpdateUserAddressValidator() 
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Address identification is required for update.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Address title is required.")
                .MaximumLength(50).WithMessage("Title cannot exceed 50 characters.");

            RuleFor(x => x.Country)
                .NotEmpty().WithMessage("Country is required.");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required.");

            RuleFor(x => x.FullAddress)
                .NotEmpty().WithMessage("Full address details are required.");
        }
    }
}
