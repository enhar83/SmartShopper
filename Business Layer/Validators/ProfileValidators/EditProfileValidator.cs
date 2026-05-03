using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.ProfileDtos;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Business_Layer.Validators.ProfileValidators
{
    public class EditProfileValidator:AbstractValidator<EditProfileDto>
    {
        public EditProfileValidator() 
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Ad alanı boş geçilemez");

            RuleFor(x => x.Surname)
                .NotEmpty().WithMessage("Soyad alanı boş geçilemez");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Kullanıcı adı alanı boş geçilemez");

            When(x => x.UserImage != null, () =>
            {
                RuleFor(x => x.UserImage)
                    .Must(BeAValidImage!).WithMessage("Only .jpg, .jpeg, .png and .webp formats are allowed.")
                    .Must(BeAValidSize!).WithMessage("Photo size must be less than 2MB.");
            });
        }

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
