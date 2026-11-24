namespace BookVerse.Domain.Entities;
using BookVerse.Domain.Common;
using BookVerse.Domain.ValueObjects;
public sealed class Book : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;
    public string Isbn { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public double Rating { get; private set; } = 5.0;
    public bool IsActive { get; private set; } = true;
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    private Book() { }
    private Book(string title, string author, Isbn isbn, string category, decimal price, int stockQuantity)
    {
        Title = title;
        Author = author;
        Isbn = isbn.Value;
        Category = category;
        Price = price;
        StockQuantity = stockQuantity;
    }
    public static Result<Book> Create(
        string title,
        string author,
        string rawIsbn,
        string category,
        decimal price,
        int initialStock)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length < 3)
            return Result.Failure<Book>(new Error("Book.InvalidTitle", "عنوان کتاب باید حداقل ۳ کاراکتر باشد."));

        if (string.IsNullOrWhiteSpace(author))
            return Result.Failure<Book>(new Error("Book.InvalidAuthor", "نام نویسنده الزامی است."));

        if (price <= 0)
            return Result.Failure<Book>(new Error("Book.InvalidPrice", "قیمت کتاب باید بزرگتر از صفر باشد."));

        if (initialStock < 0)
            return Result.Failure<Book>(new Error("Book.NegativeStock", "موجودی اولیه نمی‌تواند منفی باشد."));

        var isbnResult = ValueObjects.Isbn.Create(rawIsbn);
        if (isbnResult.IsFailure)
            return Result.Failure<Book>(isbnResult.Error);

        var book = new Book(title.Trim(), author.Trim(), isbnResult.Value, category.Trim(), price, initialStock);
        return Result.Success(book);
    }
    public Result ReduceStock(int quantity)
    {
        if (quantity <= 0)
            return Result.Failure(new Error("Book.InvalidQuantity", "تعداد باید بزرگتر از صفر باشد."));

        if (StockQuantity < quantity)
            return Result.Failure(BookErrors.InsufficientStock(Title, StockQuantity));

        StockQuantity -= quantity;
        UpdatedAtUtc = DateTime.UtcNow;
        return Result.Success();
    }
    public Result AddStock(int quantity)
    {
        if (quantity <= 0)
            return Result.Failure(new Error("Book.InvalidQuantity", "مقدار شارژ انبار باید مثبت باشد."));

        StockQuantity += quantity;
        UpdatedAtUtc = DateTime.UtcNow;
        return Result.Success();
    }
}

