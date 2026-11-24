namespace BookVerse.Api.Controllers;

using BookVerse.Application.DTOs;
using BookVerse.Application.Services;
using BookVerse.Infrastructure.Caching;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


[Route("api/v1/[controller]")]
[Authorize]
public sealed class OrdersController : ApiControllerBase
{
    private const string DefaultCatalogCacheKey = "books:catalog:page:1";

    private readonly IOrderService _orderService;
    private readonly ICacheService _cacheService;

    public OrdersController(IOrderService orderService, ICacheService cacheService)
    {
        _orderService = orderService;
        _cacheService = cacheService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        [FromServices] IValidator<CreateOrderRequest> validator,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return HandleValidationFailure(validation);

        var result = await _orderService.CreateOrderAsync(request, cancellationToken);
        if (result.IsFailure)
            return HandleFailure(result);

        await _cacheService.RemoveAsync(DefaultCatalogCacheKey, cancellationToken);

        return CreatedAtAction(nameof(GetCustomerOrders), new { customerId = result.Value.CustomerId }, result.Value);
    }

    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerOrders(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var result = await _orderService.GetCustomerOrdersAsync(customerId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }
}