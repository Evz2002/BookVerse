namespace BookVerse.Application.DTOs;
public sealed record BookDto(
    Guid Id,
    string Title,
    string Author,
    string Isbn,
    string Category,
    decimal Price,
    int StockQuantity,
    double Rating);
public sealed record GetCatalogQuery(
    string? SearchTerm = null,
    string? Category = null,
    string? SortBy = "popular",
    int PageNumber = 1,
    int PageSize = 12);
public sealed record CreateBookRequest(
    string Title,
    string Author,
    string Isbn,
    string Category,
    decimal Price,
    int StockQuantity);
public sealed record AdjustStockRequest(
    Guid BookId,
    int QuantityChange);