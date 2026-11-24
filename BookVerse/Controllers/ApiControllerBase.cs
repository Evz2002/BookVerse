namespace BookVerse.Api.Controllers;

using BookVerse.Domain.Common;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// تبدیل خطای منطقی Result به کد وضعیت HTTP مناسب (404, 409, 400)
    /// </summary>
    protected IActionResult HandleFailure(Result result)
    {
        var statusCode = result.Error.Code switch
        {
            "Book.NotFound" => StatusCodes.Status404NotFound,
            "User.InvalidCredentials" => StatusCodes.Status401Unauthorized,
            "Book.InsufficientStock" or "Book.DuplicateIsbn" or "User.DuplicateEmail" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        return Problem(
            title: result.Error.Description,
            statusCode: statusCode,
            extensions: new Dictionary<string, object?>
            {
                ["errorCode"] = result.Error.Code
            });
    }

    /// <summary>
    /// تبدیل خطاهای FluentValidation به خروجی استاندارد 400 Bad Request
    /// </summary>
    protected IActionResult HandleValidationFailure(ValidationResult validationResult)
    {
        var errors = validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        return ValidationProblem(new ValidationProblemDetails(errors)
        {
            Title = "اطلاعات ورودی نامعتبر است.",
            Status = StatusCodes.Status400BadRequest
        });
    }
}