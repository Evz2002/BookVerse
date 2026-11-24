namespace BookVerse.UnitTests.Domain;

using BookVerse.Domain.ValueObjects;
using FluentAssertions;

public sealed class IsbnTests
{
    [Fact]
    public void Create_ShouldReturnSuccessAndRemoveHyphens_WhenIsbnIsValid()
    {
        var rawIsbn = "978-0-13-449416-6";
        var result = Isbn.Create(rawIsbn);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("9780134494166");
    }

    [Theory]
    [InlineData("12345")]             
    [InlineData("9780134494166999")]  
    [InlineData("978013449416A")]     
    public void Create_ShouldReturnFailure_WhenIsbnFormatIsInvalid(string invalidIsbn)
    {
      
        var result = Isbn.Create(invalidIsbn);

     
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Book.InvalidIsbnFormat");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldReturnFailure_WhenIsbnIsEmpty(string? emptyIsbn)
    {
        
        var result = Isbn.Create(emptyIsbn);
  
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Book.EmptyIsbn");
    }
}