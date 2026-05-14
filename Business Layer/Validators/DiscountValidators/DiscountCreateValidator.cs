using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.DiscountDtos;
using FluentValidation;
using static Entity_Layer.Discount;

namespace Business_Layer.Validators.DiscountValidators
{
    public class DiscountCreateValidator:AbstractValidator<DiscountCreateDto>
    {
        public DiscountCreateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Discount name cannot be empty.")
                .MaximumLength(150).WithMessage("Discount name must not exceed 150 characters.");

            RuleFor(x => x.Value)
                .GreaterThan(0).WithMessage("Discount value must be greater than zero.");

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate).WithMessage("End date must be after the start date.");

            RuleFor(x => x.Value)
                .LessThanOrEqualTo(100).When(x => x.Type == DiscountType.Percentage)
                .WithMessage("Percentage discount value cannot exceed 100.");

            RuleFor(x => x.MaxDiscountAmount)
                .Null().When(x => x.Type == DiscountType.FixedAmount)
                .WithMessage("Maximum discount amount cannot be set for fixed amount discounts.");
        }
    }
}
