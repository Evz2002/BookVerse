namespace BookVerse.Domain.Entities;

using BookVerse.Domain.Common;

public sealed class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid BookId { get; private set; }
    public string BookTitleSnapshot { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    private OrderItem() { } 
    internal OrderItem(Guid orderId, Guid bookId, string bookTitleSnapshot, decimal unitPrice, int quantity)
    {
        OrderId = orderId;
        BookId = bookId;
        BookTitleSnapshot = bookTitleSnapshot;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }
    internal void IncreaseQuantity(int additionalQuantity)
    {
        Quantity += additionalQuantity;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}

