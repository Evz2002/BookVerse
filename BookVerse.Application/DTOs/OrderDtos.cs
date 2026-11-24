namespace BookVerse.Application.DTOs;
public sealed record OrderItemRequest(Guid BookId, int Quantity);
public sealed record CreateOrderRequest(
    Guid CustomerId,
    string ShippingAddress,
    string? CouponCode,
    IReadOnlyList<OrderItemRequest> Items);
public sealed record OrderItemDto(
    Guid BookId,
    string BookTitle,
    int Quantity,
    decimal UnitPrice,
    decimal TotalLinePrice);
public sealed record OrderResponseDto(
    Guid OrderId,
    Guid CustomerId,
    string ShippingAddress,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAtUtc,
    IReadOnlyList<OrderItemDto> Items);