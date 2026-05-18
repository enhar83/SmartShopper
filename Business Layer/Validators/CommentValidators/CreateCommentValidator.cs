using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Layer.Dtos.CommentDtos;
using FluentValidation;

namespace Business_Layer.Validators.CommentValidators
{
    public class CreateCommentValidator : AbstractValidator<CreateCommentDto>
    {
        public CreateCommentValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product information could not be retrieved. Please refresh the page.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Please add a title to your comment.")
                .MaximumLength(150).WithMessage("The title can be a maximum of 150 characters.");

            RuleFor(x => x.Text)
                .NotEmpty().WithMessage("The comment section cannot be left blank.")
                .MinimumLength(10).WithMessage("Your comment must be at least 10 characters long; please provide some additional details.")
                .MaximumLength(1000).WithMessage("Your comment can be a maximum of 1000 characters.");

            RuleFor(x => x.Rating)
                .InclusiveBetween((byte)1, (byte)5).WithMessage("Please provide a valid rating between 1 and 5.");
        }
    }
}
