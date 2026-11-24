namespace BookVerse.Application.Validators;
using BookVerse.Application.DTOs;
using FluentValidation;
public sealed class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("شناسه مشتری نامعتبر است.");

        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage("آدرس تحویل الزامی است.")
            .MinimumLength(10).WithMessage("آدرس تحویل باید حداقل ۱۰ کاراکتر باشد.")
            .MaximumLength(500).WithMessage("آدرس تحویل بیش از حد طولانی است.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("سبد خرید نمی‌تواند خالی باشد.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.BookId)
                .NotEmpty().WithMessage("شناسه کتاب نامعتبر است.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("تعداد هر کتاب باید حداقل ۱ عدد باشد.")
                .LessThanOrEqualTo(50).WithMessage("حداکثر تعداد مجاز خرید هر کتاب ۵۰ عدد است.");
        });
    }
}