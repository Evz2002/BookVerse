namespace BookVerse.Application.Services;
using BookVerse.Application.DTOs;
using BookVerse.Domain.Common;
using BookVerse.Domain.Entities;
using BookVerse.Domain.Repositories;
using Microsoft.Extensions.Logging;
public sealed class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BookService> _logger;
    public BookService(
        IBookRepository bookRepository,
        IUnitOfWork unitOfWork,
        ILogger<BookService> logger)
    {
        _bookRepository = bookRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    public async Task<Result<PagedResult<BookDto>>> GetCatalogAsync(
        GetCatalogQuery query,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Fetching book catalog. SearchTerm: {SearchTerm}, Category: {Category}, Page: {PageNumber}",
            query.SearchTerm ?? "None",
            query.Category ?? "ALL",
            query.PageNumber);

        var (books, totalCount) = await _bookRepository.GetCatalogAsync(
            query.SearchTerm,
            query.Category,
            query.SortBy,
            query.PageNumber,
            query.PageSize,
            cancellationToken);

        var dtos = books
            .Select(b => new BookDto(b.Id, b.Title, b.Author, b.Isbn, b.Category, b.Price, b.StockQuantity, b.Rating))
            .ToList();

        return Result.Success(new PagedResult<BookDto>(dtos, totalCount, query.PageNumber, query.PageSize));
    }

    public async Task<Result<BookDto>> CreateBookAsync(
        CreateBookRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating book with ISBN {Isbn}", request.Isbn);
        var bookResult = Book.Create(
            request.Title,
            request.Author,
            request.Isbn,
            request.Category,
            request.Price,
            request.StockQuantity);

        if (bookResult.IsFailure)
        {
            _logger.LogWarning(
                "Book creation rejected by Domain: {ErrorCode} - {ErrorMessage}",
                bookResult.Error.Code,
                bookResult.Error.Description);

            return Result.Failure<BookDto>(bookResult.Error);
        }

        var book = bookResult.Value;

        if (await _bookRepository.ExistsByIsbnAsync(book.Isbn, cancellationToken))
        {
            _logger.LogWarning("Book creation rejected: Duplicate ISBN {Isbn}", book.Isbn);
            return Result.Failure<BookDto>(BookErrors.DuplicateIsbn);
        }

        _bookRepository.Add(book);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Book {BookId} ({Title}) created with stock {Stock}", book.Id, book.Title, book.StockQuantity);

        return Result.Success(new BookDto(
            book.Id, book.Title, book.Author, book.Isbn, book.Category, book.Price, book.StockQuantity, book.Rating));
    }

    public async Task<Result<BookDto>> AdjustStockAsync(
        AdjustStockRequest request,
        CancellationToken cancellationToken = default)
    {
        var book = await _bookRepository.GetByIdAsync(request.BookId, cancellationToken);
        if (book is null)
        {
            _logger.LogWarning("Stock adjustment failed: Book {BookId} not found", request.BookId);
            return Result.Failure<BookDto>(BookErrors.NotFound);
        }
        var operationResult = request.QuantityChange >= 0
            ? book.AddStock(request.QuantityChange)
            : book.ReduceStock(Math.Abs(request.QuantityChange));

        if (operationResult.IsFailure)
        {
            _logger.LogWarning(
                "Stock adjustment blocked for Book {BookId}. Change: {Change}, CurrentStock: {Stock}",
                book.Id,
                request.QuantityChange,
                book.StockQuantity);

            return Result.Failure<BookDto>(operationResult.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Stock adjusted for Book {BookId}. NewStock: {NewStock}", book.Id, book.StockQuantity);

        return Result.Success(new BookDto(
            book.Id, book.Title, book.Author, book.Isbn, book.Category, book.Price, book.StockQuantity, book.Rating));
    }
}