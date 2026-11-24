namespace BookVerse.Domain.ValueObjects;
using BookVerse.Domain.Common;
public sealed record Isbn
{
    public string Value { get; }
    private Isbn(string value)
    {
        Value = value;
    }
    public static Result<Isbn> Create(string? rawIsbn)
    {
        if (string.IsNullOrWhiteSpace(rawIsbn))
            return Result.Failure<Isbn>(new Error("Book.EmptyIsbn", "شابک کتاب الزامی است."));

        var normalized = rawIsbn.Replace("-", "").Trim();

        if (normalized.Length != 13 || !normalized.All(char.IsDigit))
            return Result.Failure<Isbn>(new Error("Book.InvalidIsbnFormat", "شابک باید دقیقاً ۱۳ رقم عددی باشد."));

        return Result.Success(new Isbn(normalized));
    }
}