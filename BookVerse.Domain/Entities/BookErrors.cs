namespace BookVerse.Domain.Entities;
using BookVerse.Domain.Common;

public static class BookErrors
{
    public static readonly Error NotFound = new("Book.NotFound", "کتاب مورد نظر یافت نشد.");
    public static readonly Error DuplicateIsbn = new("Book.DuplicateIsbn", "کتابی با این شابک قبلاً ثبت شده است.");
    public static Error InsufficientStock(string title, int available) =>
        new("Book.InsufficientStock", $"موجودی کتاب «{title}» کافی نیست. موجودی فعلی: {available} عدد.");
}

