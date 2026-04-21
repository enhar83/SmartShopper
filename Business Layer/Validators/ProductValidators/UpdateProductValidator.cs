using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.ProductDtos;
using FluentValidation;

namespace Business_Layer.Validators.ProductValidators
{
    public class UpdateProductValidator:AbstractValidator<UpdateProductDto>
    {
        public UpdateProductValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Product name is mandatory.")
                .MinimumLength(2).WithMessage("The product name must be at least two characters long.")
                .MaximumLength(50).WithMessage("The product name cannot exceed 50 characters.");

            RuleFor(c => c.Description)
                .NotEmpty().WithMessage("Product description is mandatory.")
                .MinimumLength(2).WithMessage("The product description must be at least two characters long.")
                .MaximumLength(250).WithMessage("The product description cannot exceed 250 characters.");

            RuleFor(p => p.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");

            RuleFor(p => p.Stock)
                .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative.");

            RuleFor(p => p.SubCategoryId)
                .NotEmpty().WithMessage("You must select a sub-category for the product.");

            RuleFor(p => p.Gender)
                .IsInEnum().When(p => p.Gender.HasValue).WithMessage("Please select a valid gender type.");
        }
    }
}
