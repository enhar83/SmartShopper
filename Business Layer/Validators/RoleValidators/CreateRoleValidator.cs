using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.RoleDtos;
using FluentValidation;

namespace Business_Layer.Validators.RoleValidators
{
    public class CreateRoleValidator:AbstractValidator<CreateRoleDto>
    {
        public CreateRoleValidator()
        {
            RuleFor(r => r.RoleName)
                .NotEmpty().WithMessage("Role name cannot be empty.")
                .MinimumLength(2).WithMessage("Role name must be at least 2 characters long.")
                .MaximumLength(50).WithMessage("Role name cannot exceed 50 characters.");

            RuleFor(r => r.Description)
                .NotEmpty().WithMessage("Please provide a description for this role.")
                .MinimumLength(5).WithMessage("Description should be at least 5 characters long to be informative.")
                .MaximumLength(200).WithMessage("Description cannot exceed 200 characters.");
        }
    }
}
