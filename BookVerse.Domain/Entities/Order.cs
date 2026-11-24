namespace BookVerse.Domain.Entities;
using BookVerse.Domain.Common;
public sealed class Order : BaseEntity
{
    private readonly List<OrderItem> _items = new();

    public Guid CustomerId { get; private set; }
    public string ShippingAddress { get; private set; } = string.Empty;
    public decimal Subtotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    private Order() { } 
    private Order(Guid customerId, string shippingAddress)
    {
        CustomerId = customerId;
        ShippingAddress = shippingAddress;
    }
    public static Result<Order> Create(Guid customerId, string shippingAddress)
    {
        if (customerId == Guid.Empty)
            return Result.Failure<Order>(new Error("Order.InvalidCustomer", "شناسه مشتری نامعتبر است."));

        if (string.IsNullOrWhiteSpace(shippingAddress) || shippingAddress.Trim().Length < 10)
            return Result.Failure<Order>(new Error("Order.InvalidAddress", "آدرس تحویل باید حداقل ۱۰ کاراکتر باشد."));

        return Result.Success(new Order(customerId, shippingAddress.Trim()));
    }
    public Result AddItem(Book book, int quantity)
    {
        if (Status != OrderStatus.Pending)
            return Result.Failure(new Error("Order.NotModifiable", "سفارش نهایی‌شده قابل تغییر نیست."));

        if (quantity <= 0)
            return Result.Failure(new Error("Order.InvalidQuantity", "تعداد کتاب باید حداقل ۱ باشد."));

        var existingItem = _items.FirstOrDefault(i => i.BookId == book.Id);
        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            _items.Add(new OrderItem(Id, book.Id, book.Title, book.Price, quantity));
        }

        RecalculateTotals();
        return Result.Success();
    }
    public Result ApplyCoupon(string? couponCode)
    {
        if (string.IsNullOrWhiteSpace(couponCode))
            return Result.Success();

        if (couponCode.Trim().Equals("CLEAN20", StringComparison.OrdinalIgnoreCase))
        {
            DiscountAmount = Math.Round(Subtotal * 0.20m, 0);
            RecalculateTotals();
            return Result.Success();
        }

        return Result.Failure(new Error("Order.InvalidCoupon", "کد تخفیف معتبر نیست."));
    }
    public Result Cancel()
    {
        if (Status == OrderStatus.Shipped)
            return Result.Failure(new Error("Order.CannotCancelShipped", "سفارش ارسال‌شده قابل لغو نیست."));

        if (Status == OrderStatus.Cancelled)
            return Result.Failure(new Error("Order.AlreadyCancelled", "این سفارش قبلاً لغو شده است."));

        Status = OrderStatus.Cancelled;
        UpdatedAtUtc = DateTime.UtcNow;
        return Result.Success();
    }
    private void RecalculateTotals()
    {
        Subtotal = _items.Sum(i => i.UnitPrice * i.Quantity);
        TotalAmount = Math.Max(0, Subtotal - DiscountAmount);
        UpdatedAtUtc = DateTime.UtcNow;
    }
}

