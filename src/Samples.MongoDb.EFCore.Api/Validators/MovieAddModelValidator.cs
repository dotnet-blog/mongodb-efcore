using FluentValidation;
using Samples.MongoDb.EFCore.Api.Dtos.MediaLibrary;

namespace Samples.MongoDb.EFCore.Api.Validators
{
    public class MovieAddModelValidator : AbstractValidator<MovieAddModel>
    {
        public MovieAddModelValidator()
        {
            RuleFor(m => m.Title).NotNull().NotEmpty().MaximumLength(1);
            RuleFor(m => m.Synopsis).MaximumLength(1024);
        }
    }
}
