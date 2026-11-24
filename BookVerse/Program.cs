using System.Text;
using BookVerse.Api.ExceptionHandlers;
using BookVerse.Application.Services;
using BookVerse.Application.Validators;
using BookVerse.Infrastructure;
using BookVerse.Infrastructure.Security;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ۱. Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// ۲. Application Services & Validators
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();

// ۳. خواندن تنظیمات JWT و ثبت لایه Infrastructure
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings section is missing in appsettings.json.");

var sqlConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection is missing.");
var redisConnection = builder.Configuration.GetConnectionString("Redis");

builder.Services.AddInfrastructure(sqlConnection, jwtSettings, redisConnection);

// ۴. پیکربندی احراز هویت JWT Bearer
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

builder.Services.AddAuthorization();

// ۵. Global Exception Handling & RFC 7807
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ۶. Controllers + دکمه قفل Authorize در Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "BookVerse Enterprise API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "فقط خود توکن JWT را اینجا وارد کنید (نیازی به نوشتن کلمه Bearer نیست)."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});
// تعریف سیاست CORS برای اجازه ارتباط فرانت‌اند با API
builder.Services.AddCors(options =>
{
    options.AddPolicy("BookVerseFrontendPolicy", policy =>
    {
        // برای محیط توسعه (تست فایل HTML محلی یا Live Server روی VS Code):
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();

        // نکته سینیور برای محیط واقعی (Production):
        // در سرور واقعی هرگز AllowAnyOrigin نگذار؛ فقط دامنه واقعی سایتت را مجاز کن:
        // policy.WithOrigins("https://bookverse.ir").AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// ۷. ترتیب طلایی Middleware Pipeline
app.UseExceptionHandler();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("BookVerseFrontendPolicy");

// نکته حیاتی: اول احراز هویت (کیستی؟)، بعد مجوز دسترسی (آیا اجازه داری؟)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();