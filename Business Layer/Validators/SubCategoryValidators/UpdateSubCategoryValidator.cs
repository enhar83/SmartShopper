using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.SubCategoryDtos;
using FluentValidation;

namespace Business_Layer.Validators.SubCategoryValidators
{
    public class UpdateSubCategoryValidator:AbstractValidator<UpdateSubCategoryDto>
    {
        public UpdateSubCategoryValidator()
        {
            RuleFor(s => s.Name)
                .NotEmpty().WithMessage("Subategory name is mandatory.")
                .MinimumLength(2).WithMessage("The subcategory name must be at least two characters long.")
                .MaximumLength(50).WithMessage("The subcategory name cannot exceed 50 characters.");
        }
    }
}
