namespace BookVerse.Application.Services;
using BookVerse.Application.DTOs;
using BookVerse.Domain.Common;
using BookVerse.Domain.Entities;
using BookVerse.Domain.Repositories;
using Microsoft.Extensions.Logging;
public sealed class OrderService : IOrderService
{
    private readonly IBookRepository _bookRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderService> _logger;
    public OrderService(
        IBookRepository bookRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ILogger<OrderService> logger)
    {
        _bookRepository = bookRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    public async Task<Result<OrderResponseDto>> CreateOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting order creation for Customer {CustomerId} with {ItemCount} items",
            request.CustomerId,
            request.Items?.Count ?? 0);

        if (request.Items is null || request.Items.Count == 0)
        {
            _logger.LogWarning("Order rejected: Empty basket for Customer {CustomerId}", request.CustomerId);
            return Result.Failure<OrderResponseDto>(new Error("Order.EmptyBasket", "سبد خرید خالی است."));
        }
        var orderResult = Order.Create(request.CustomerId, request.ShippingAddress);
        if (orderResult.IsFailure)
        {
            _logger.LogWarning("Order creation failed for Customer {CustomerId}: {Error}", request.CustomerId, orderResult.Error.Code);
            return Result.Failure<OrderResponseDto>(orderResult.Error);
        }

        var order = orderResult.Value;

        var bookIds = request.Items.Select(i => i.BookId).Distinct().ToList();
        var booksInDb = await _bookRepository.GetByIdsAsync(bookIds, cancellationToken);
        var booksMap = booksInDb.ToDictionary(b => b.Id);

        foreach (var itemRequest in request.Items)
        {
            if (!booksMap.TryGetValue(itemRequest.BookId, out var book))
            {
                _logger.LogWarning("Order failed: Book {BookId} not found", itemRequest.BookId);
                return Result.Failure<OrderResponseDto>(BookErrors.NotFound);
            }

            var stockResult = book.ReduceStock(itemRequest.Quantity);
            if (stockResult.IsFailure)
            {
                _logger.LogWarning(
                    "Order failed: Insufficient stock for Book {BookId}. Requested: {Requested}, Available: {Available}",
                    book.Id,
                    itemRequest.Quantity,
                    book.StockQuantity);

                return Result.Failure<OrderResponseDto>(stockResult.Error);
            }

            var addItemResult = order.AddItem(book, itemRequest.Quantity);
            if (addItemResult.IsFailure)
                return Result.Failure<OrderResponseDto>(addItemResult.Error);
        }

        var couponResult = order.ApplyCoupon(request.CouponCode);
        if (couponResult.IsFailure)
        {
            _logger.LogWarning("Invalid coupon {CouponCode} used by Customer {CustomerId}", request.CouponCode, request.CustomerId);
            return Result.Failure<OrderResponseDto>(couponResult.Error);
        }
        _orderRepository.Add(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Order {OrderId} placed successfully for Customer {CustomerId}. FinalTotal: {TotalAmount}",
            order.Id,
            order.CustomerId,
            order.TotalAmount);

        return Result.Success(MapToDto(order));
    }
    public async Task<Result<IReadOnlyList<OrderResponseDto>>> GetCustomerOrdersAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching orders for Customer {CustomerId}", customerId);

        var orders = await _orderRepository.GetCustomerOrdersAsync(customerId, cancellationToken);
        var dtos = orders.Select(MapToDto).ToList();

        return Result.Success<IReadOnlyList<OrderResponseDto>>(dtos);
    }
    private static OrderResponseDto MapToDto(Order order)
    {
        var itemDtos = order.Items
            .Select(i => new OrderItemDto(
                i.BookId,
                i.BookTitleSnapshot,
                i.Quantity,
                i.UnitPrice,
                i.UnitPrice * i.Quantity))
            .ToList();

        return new OrderResponseDto(
            order.Id,
            order.CustomerId,
            order.ShippingAddress,
            order.Subtotal,
            order.DiscountAmount,
            order.TotalAmount,
            order.Status.ToString(),
            order.CreatedAtUtc,
            itemDtos);
    }
}