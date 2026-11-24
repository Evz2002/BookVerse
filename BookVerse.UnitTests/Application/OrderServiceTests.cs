namespace BookVerse.UnitTests.Application;
using BookVerse.Application.DTOs;
using BookVerse.Application.Services;
using BookVerse.Domain.Entities;
using BookVerse.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

public sealed class OrderServiceTests
{
    private readonly Mock<IBookRepository> _bookRepoMock;
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _bookRepoMock = new Mock<IBookRepository>();
        _orderRepoMock = new Mock<IOrderRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new OrderService(
            _bookRepoMock.Object,
            _orderRepoMock.Object,
            _unitOfWorkMock.Object,
            NullLogger<OrderService>.Instance);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldReturnFailureAndNotSave_WhenBookStockIsInsufficient()
    {
        var book = Book.Create(
            "Clean Architecture",
            "Robert C. Martin",
            "9780134494166",
            "Architecture",
            500_000m,
            initialStock: 2).Value;

        var request = new CreateOrderRequest(
            CustomerId: Guid.NewGuid(),
            ShippingAddress: "تهران، خیابان ولیعصر، پلاک ۱۲۴",
            CouponCode: null,
            Items: new List<OrderItemRequest>
            {
                new(book.Id, Quantity: 5)
            });
        _bookRepoMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Book> { book });

        var result = await _sut.CreateOrderAsync(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Book.InsufficientStock");
        book.StockQuantity.Should().Be(2); 

        _orderRepoMock.Verify(r => r.Add(It.IsAny<Order>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldReturnNotFound_WhenBookDoesNotExistInDatabase()
    {
        var request = new CreateOrderRequest(
            CustomerId: Guid.NewGuid(),
            ShippingAddress: "تهران، خیابان ولیعصر، پلاک ۱۲۴",
            CouponCode: null,
            Items: new List<OrderItemRequest>
            {
                new(Guid.NewGuid(), Quantity: 1)
            });

        _bookRepoMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Book>());

        var result = await _sut.CreateOrderAsync(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Book.NotFound");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldDeductStockApplyCouponAndCommit_WhenRequestIsValid()
    {
        var book = Book.Create(
            "ASP.NET Core in Action",
            "Andrew Lock",
            "9781633438620",
            "DotNet",
            600_000m,
            initialStock: 10).Value;

        var request = new CreateOrderRequest(
            CustomerId: Guid.NewGuid(),
            ShippingAddress: "تهران، میدان ونک، خیابان ملاصدرا، پلاک ۱۰",
            CouponCode: "CLEAN20",
            Items: new List<OrderItemRequest>
            {
                new(book.Id, Quantity: 2) 
            });

        _bookRepoMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Book> { book });

        var result = await _sut.CreateOrderAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Subtotal.Should().Be(1_200_000m);
        result.Value.DiscountAmount.Should().Be(240_000m);
        result.Value.TotalAmount.Should().Be(960_000m);

        book.StockQuantity.Should().Be(8);

        _orderRepoMock.Verify(r => r.Add(It.IsAny<Order>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}