namespace BookVerse.Domain.Repositories;
using BookVerse.Domain.Entities;

public interface IBookRepository
{
    Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Book>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    Task<bool> ExistsByIsbnAsync(string isbn, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Book> Items, int TotalCount)> GetCatalogAsync(
        string? searchTerm,
        string? category,
        string? sortBy,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    void Add(Book book);
}

