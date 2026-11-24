namespace BookVerse.Domain.Repositories;
using BookVerse.Domain.Entities;
public interface IOrderRepository
{
    Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetCustomerOrdersAsync(Guid customerId, CancellationToken cancellationToken = default);
    void Add(Order order);
}

