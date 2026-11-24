namespace BookVerse.Application.Services;
using BookVerse.Application.DTOs;
using BookVerse.Domain.Common;
public interface IOrderService
{
    Task<Result<OrderResponseDto>> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<OrderResponseDto>>> GetCustomerOrdersAsync(Guid customerId, CancellationToken cancellationToken = default);
}