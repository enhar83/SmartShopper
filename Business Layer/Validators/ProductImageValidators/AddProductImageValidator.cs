using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.ProductDtos;
using Core_Layer.Dtos.ProductImagesDtos;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Business_Layer.Validators.ProductImageValidators
{
    public class AddProductImageValidator : AbstractValidator<AddProductImageDto>
    {
        public AddProductImageValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required.");

            RuleFor(x => x.Photo)
                .NotNull().WithMessage("Please select a photo.")
                .Must(BeAValidImage).WithMessage("Only .jpg, .jpeg, .png and .webp formats are allowed.")
                .Must(BeAValidSize).WithMessage("Photo size must be less than 2MB.");
        }

        //format kontrolü
        private bool BeAValidImage(IFormFile file)
        {
            if (file == null) return false;
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = System.IO.Path.GetExtension(file.FileName).ToLower();
            return allowedExtensions.Contains(extension);
        }

        //boyut kontrolü
        private bool BeAValidSize(IFormFile file)
        {
            if (file == null) return false;
            return file.Length <= 2 * 1024 * 1024;
        }
    }
}
