namespace BookVerse.Api.ExceptionHandlers;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = httpContext.TraceIdentifier;

        // ۱. ثبت کامل خطا و StackTrace در فایل لاگ سرور (Serilog) — هرگز به کاربر نشان داده نمی‌شود
        _logger.LogError(
            exception,
            "Unhandled exception on {Method} {Path}. TraceId: {TraceId}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            traceId);

        // ۲. تشخیص هوشمند نوع خطا و انتخاب کد وضعیت HTTP استاندارد
        var (statusCode, title, errorCode) = exception switch
        {
            DbUpdateConcurrencyException => (
                StatusCodes.Status409Conflict,
                "موجودی کتاب در همین لحظه توسط کاربر دیگری تغییر کرد. لطفاً مجدداً تلاش کنید.",
                "Concurrency.Conflict"),

            OperationCanceledException => (
                StatusCodes.Status400BadRequest,
                "درخواست توسط کاربر لغو شد.",
                "Request.Cancelled"),

            _ => (
                StatusCodes.Status500InternalServerError,
                "خطای داخلی سرور رخ داده است. لطفاً با کد پیگیری به پشتیبانی اطلاع دهید.",
                "Server.InternalError")
        };

        // ۳. ساخت پاسخ استاندارد جهانی RFC 7807 (ProblemDetails)
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = "https://tools.ietf.org/html/rfc7807",
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = traceId;
        problemDetails.Extensions["errorCode"] = errorCode;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        // بازگرداندن true یعنی خطا کامل مدیریت شد و نباید بیشتر بالا برود
        return true;
    }
}