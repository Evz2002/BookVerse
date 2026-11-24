namespace BookVerse.UnitTests.Domain;

using BookVerse.Domain.Entities;
using FluentAssertions;

public sealed class BookTests
{
    [Fact]
    public void ReduceStock_ShouldDecreaseStockQuantity_WhenEnoughStockExists()
    {
        var book = Book.Create(
            "Clean Architecture",
            "Robert C. Martin",
            "9780134494166",
            "Architecture",
            680_000m,
            initialStock: 10).Value;

        var result = book.ReduceStock(3);

        result.IsSuccess.Should().BeTrue();
        book.StockQuantity.Should().Be(7);
    }

    [Fact]
    public void ReduceStock_ShouldReturnFailureAndKeepStockUnchanged_WhenStockIsInsufficient()
    {
        var book = Book.Create(
            "Clean Architecture",
            "Robert C. Martin",
            "9780134494166",
            "Architecture",
            680_000m,
            initialStock: 2).Value;

        var result = book.ReduceStock(5);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Book.InsufficientStock");
        book.StockQuantity.Should().Be(2);
    }
}