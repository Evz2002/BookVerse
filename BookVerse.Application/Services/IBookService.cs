namespace BookVerse.Application.Services;
using BookVerse.Application.DTOs;
using BookVerse.Domain.Common;
public interface IBookService
{
    Task<Result<PagedResult<BookDto>>> GetCatalogAsync(GetCatalogQuery query, CancellationToken cancellationToken = default);
    Task<Result<BookDto>> CreateBookAsync(CreateBookRequest request, CancellationToken cancellationToken = default);
    Task<Result<BookDto>> AdjustStockAsync(AdjustStockRequest request, CancellationToken cancellationToken = default);
}