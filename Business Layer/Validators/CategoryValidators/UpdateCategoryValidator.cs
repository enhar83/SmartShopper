using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.CategoryDtos;
using FluentValidation;

namespace Business_Layer.Validators.CategoryValidators
{
    public class UpdateCategoryValidator:AbstractValidator<UpdateCategoryDto>
    {
        public UpdateCategoryValidator() 
        {
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Category name is mandatory.")
                .MinimumLength(2).WithMessage("The category name must be at least two characters long.")
                .MaximumLength(50).WithMessage("The category name cannot exceed 50 characters.");

            RuleFor(c => c.Description)
                .NotEmpty().WithMessage("Category description is mandatory.")
                .MinimumLength(2).WithMessage("The category description must be at least two characters long.")
                .MaximumLength(250).WithMessage("The category description cannot exceed 250 characters.");
        }
    }
}
