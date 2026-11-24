namespace BookVerse.Application.Validators;
using BookVerse.Application.DTOs;
using FluentValidation;
public sealed class CreateBookRequestValidator : AbstractValidator<CreateBookRequest>
{
    public CreateBookRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان کتاب الزامی است.")
            .MinimumLength(3).WithMessage("عنوان کتاب باید حداقل ۳ کاراکتر باشد.")
            .MaximumLength(200).WithMessage("عنوان کتاب بیش از حد طولانی است.");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("نام نویسنده الزامی است.")
            .MaximumLength(150).WithMessage("نام نویسنده نمی‌تواند بیش از ۱۵۰ کاراکتر باشد.");

        RuleFor(x => x.Isbn)
            .NotEmpty().WithMessage("شابک کتاب الزامی است.")
            .Must(BeValidIsbn13).WithMessage("شابک باید دقیقاً ۱۳ رقم عددی معتبر باشد.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("دسته‌بندی کتاب الزامی است.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("قیمت کتاب باید بزرگتر از صفر باشد.");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("موجودی اولیه نمی‌تواند منفی باشد.");
    }
    private static bool BeValidIsbn13(string? isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn)) return false;
        var clean = isbn.Replace("-", "").Trim();
        return clean.Length == 13 && clean.All(char.IsDigit);
    }
}