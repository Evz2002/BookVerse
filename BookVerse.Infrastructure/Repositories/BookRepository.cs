namespace BookVerse.Infrastructure.Repositories;
using BookVerse.Domain.Entities;
using BookVerse.Domain.Repositories;
using BookVerse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public sealed class BookRepository : IBookRepository
{
    private readonly AppDbContext _dbContext;

    public BookRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Books
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Book>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await _dbContext.Books
            .Where(b => idList.Contains(b.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByIsbnAsync(string isbn, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Books
            .AnyAsync(b => b.Isbn == isbn, cancellationToken);
    }
    public async Task<(IReadOnlyList<Book> Items, int TotalCount)> GetCatalogAsync(
        string? searchTerm,
        string? category,
        string? sortBy,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Book> query = _dbContext.Books.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(b =>
                b.Title.Contains(term) ||
                b.Author.Contains(term) ||
                b.Isbn.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("ALL", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(b => b.Category == category);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = sortBy switch
        {
            "price-asc" => query.OrderBy(b => b.Price),
            "price-desc" => query.OrderByDescending(b => b.Price),
            "rating" => query.OrderByDescending(b => b.Rating),
            _ => query.OrderByDescending(b => b.CreatedAtUtc)
        };

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
    public void Add(Book book)
    {
        _dbContext.Books.Add(book);
    }
}