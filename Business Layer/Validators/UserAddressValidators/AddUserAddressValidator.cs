using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.UserAddressDtos;
using FluentValidation;
using Org.BouncyCastle.Asn1.Mozilla;

namespace Business_Layer.Validators.UserAddressValidators
{
    public class AddUserAddressValidator:AbstractValidator<AddUserAddressDto>
    {
        public AddUserAddressValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Address title is required.")
                .MaximumLength(50).WithMessage("Title cannot exceed 50 characters.");

            RuleFor(x => x.Country)
                .NotEmpty().WithMessage("Country is required.");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required.");

            RuleFor(x => x.FullAddress)
                .NotEmpty().WithMessage("Full address is required.")
                .MinimumLength(10).WithMessage("Please provide a more detailed address.");
        }
    }
}
