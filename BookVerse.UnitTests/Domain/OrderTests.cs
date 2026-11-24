namespace BookVerse.UnitTests.Domain;

using BookVerse.Domain.Entities;
using FluentAssertions;

public sealed class OrderTests
{
    [Fact]
    public void ApplyCoupon_ShouldCalculate20PercentDiscount_WhenCouponIsClean20()
    {

        var book = Book.Create(
            "Unit Testing Principles",
            "Vladimir Khorikov",
            "9781617296277",
            "Testing",
            500_000m,
            initialStock: 10).Value;

        var order = Order.Create(Guid.NewGuid(), "تهران، خیابان ولیعصر، پلاک ۱۲۴").Value;
        order.AddItem(book, quantity: 2); 

        var couponResult = order.ApplyCoupon("CLEAN20");

        couponResult.IsSuccess.Should().BeTrue();
        order.Subtotal.Should().Be(1_000_000m);
        order.DiscountAmount.Should().Be(200_000m);
        order.TotalAmount.Should().Be(800_000m);
    }

    [Fact]
    public void ApplyCoupon_ShouldReturnFailure_WhenCouponCodeIsInvalid()
    {

        var order = Order.Create(Guid.NewGuid(), "تهران، خیابان ولیعصر، پلاک ۱۲۴").Value;
        var couponResult = order.ApplyCoupon("WRONG_CODE");

        couponResult.IsFailure.Should().BeTrue();
        couponResult.Error.Code.Should().Be("Order.InvalidCoupon");
        order.DiscountAmount.Should().Be(0);
    }

    [Fact]
    public void Cancel_ShouldChangeStatusToCancelled_WhenOrderIsPending()
    {
        var order = Order.Create(Guid.NewGuid(), "تهران، خیابان ولیعصر، پلاک ۱۲۴").Value;

        var result = order.Cancel();
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }
}