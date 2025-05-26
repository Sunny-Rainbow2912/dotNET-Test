using FluentValidation;
using Test.Models.Dto;

namespace Test.Utils
{
    public class Validator<T> : AbstractValidator<T>
    {
        public Validator()
        {
            // Add common validation rules here if needed
        }

        public void ValidateModel(T model)
        {
            var result = Validate(model);
            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }
    }
}