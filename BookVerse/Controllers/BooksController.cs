namespace BookVerse.Api.Controllers;

using BookVerse.Application.DTOs;
using BookVerse.Application.Services;
using BookVerse.Infrastructure.Caching;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[Route("api/v1/[controller]")]
public sealed class BooksController : ApiControllerBase
{
    private const string DefaultCatalogCacheKey = "books:catalog:page:1";

    private readonly IBookService _bookService;
    private readonly ICacheService _cacheService;

    public BooksController(IBookService bookService, ICacheService cacheService)
    {
        _bookService = bookService;
        _cacheService = cacheService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<BookDto>), StatusCodes.Status200OK)]
    [AllowAnonymous]
    public async Task<IActionResult> GetCatalog(
        [FromQuery] GetCatalogQuery query,
        CancellationToken cancellationToken)
    {
        var isDefaultCatalog = string.IsNullOrWhiteSpace(query.SearchTerm)
            && (string.IsNullOrWhiteSpace(query.Category) || query.Category == "ALL")
            && query.PageNumber == 1
            && query.PageSize == 12;

        if (isDefaultCatalog)
        {
            var cached = await _cacheService.GetAsync<PagedResult<BookDto>>(DefaultCatalogCacheKey, cancellationToken);
            if (cached is not null)
                return Ok(cached);
        }

        var result = await _bookService.GetCatalogAsync(query, cancellationToken);
        if (result.IsFailure)
            return HandleFailure(result);

        if (isDefaultCatalog)
        {
            await _cacheService.SetAsync(DefaultCatalogCacheKey, result.Value, TimeSpan.FromMinutes(5), cancellationToken);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateBook(
        [FromBody] CreateBookRequest request,
        [FromServices] IValidator<CreateBookRequest> validator,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return HandleValidationFailure(validation);

        var result = await _bookService.CreateBookAsync(request, cancellationToken);
        if (result.IsFailure)
            return HandleFailure(result);
        await _cacheService.RemoveAsync(DefaultCatalogCacheKey, cancellationToken);

        return CreatedAtAction(nameof(GetCatalog), new { id = result.Value.Id }, result.Value);
    }

    [HttpPatch("{id:guid}/stock")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdjustStock(
        Guid id,
        [FromBody] int quantityChange,
        CancellationToken cancellationToken)
    {
        var result = await _bookService.AdjustStockAsync(new AdjustStockRequest(id, quantityChange), cancellationToken);
        if (result.IsFailure)
            return HandleFailure(result);

        await _cacheService.RemoveAsync(DefaultCatalogCacheKey, cancellationToken);
        return Ok(result.Value);
    }
}