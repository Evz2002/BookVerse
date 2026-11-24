# BookVerse — ASP.NET Core Clean Architecture API

An enterprise-grade online bookstore backend built with **.NET 9** and **Clean Architecture**, designed around Domain-Driven Design (DDD) principles, structured logging, global exception handling, distributed caching, JWT role-based security, and isolated unit testing.

> **Disclaimer (Backend-Only Portfolio Project):**
> I am a **.NET Backend Developer**. All backend architecture, domain logic, persistence, security, logging, exception handling, and unit tests in this repository were engineered by me. The lightweight storefront interface in `BookVerse/wwwroot/index.html` was **100% AI-generated** solely as a convenient test client to interact with the live API endpoints—I had no role in designing or writing the frontend code.

## Architecture Overview

The solution strictly enforces the **Clean Architecture Dependency Rule**, keeping the core business rules isolated from external frameworks and databases:

| Layer / Project | Responsibility | Dependencies |
| :--- | :--- | :--- |
| **`BookVerse.Domain`** | Rich Domain Entities (`Book`, `Order`, `OrderItem`, `User`), Value Objects (`Isbn`), `Result<T>` pattern, and Repository/UnitOfWork interfaces. | **Zero external packages** |
| **`BookVerse.Application`** | Use-case orchestration (`BookService`, `OrderService`, `AuthService`), DTOs, `FluentValidation` rules, and security abstractions (`IPasswordHasher`, `IJwtTokenGenerator`). | `BookVerse.Domain` |
| **`BookVerse.Infrastructure`** | EF Core 9 (`AppDbContext`), Fluent API configurations, SQL Server repositories, `RowVersion` concurrency tokens, Redis/Memory `Cache-Aside` service, PBKDF2 password hashing, and JWT token generation. | `BookVerse.Application` |
| **`BookVerse` (Web API)** | Thin RESTful controllers, `GlobalExceptionHandler` (`IExceptionHandler` + RFC 7807 `ProblemDetails`), Serilog request/file logging, CORS, and Swagger Bearer authentication. | `BookVerse.Application`, `BookVerse.Infrastructure` |
| **`BookVerse.UnitTests`** | Isolated domain and service tests using **xUnit**, **Moq**, and **FluentAssertions** following the AAA pattern. | `BookVerse.Domain`, `BookVerse.Application` |

## Key Backend Highlights

* **Rich Domain Model & Encapsulation:** Private setters, factory methods (`Book.Create`, `Order.Create`), encapsulated `_items` collections (`IReadOnlyCollection`), and price/title snapshots on `OrderItem`.
* **Result Pattern vs. Exceptions:** Predictable business rule violations (e.g., `Book.InsufficientStock`, `Book.DuplicateIsbn`, `Order.InvalidCoupon`) return lightweight `Result<T>` objects instead of throwing expensive exceptions.
* **Global Exception Handling (RFC 7807):** Centralized `IExceptionHandler` catches unhandled exceptions and `DbUpdateConcurrencyException`, logs full stack traces to Serilog, and returns safe `ProblemDetails` responses with a `traceId`.
* **Structured Logging (Serilog):** Structured message templates written to Console and daily rolling log files (`Logs/bookverse-.log`) alongside HTTP request telemetry.
* **Optimistic Concurrency & Atomic Transactions:** `RowVersion` protects inventory against concurrent checkout race conditions, while `IUnitOfWork` ensures stock deduction and order creation commit atomically.
* **Caching (Cache-Aside):** `RedisCacheService` caches catalog queries with automatic fallback to `DistributedMemoryCache` if Redis is not configured, plus cache invalidation on writes.
* **Security:** Salted PBKDF2 (`SHA512`) password hashing and `HS256` JWT Bearer authentication with `Admin` and `Customer` role enforcement.

## API Endpoints

| Method | Endpoint | Access | Description |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/auth/register` | Public | Register a new user and receive a JWT token |
| `POST` | `/api/v1/auth/login` | Public | Authenticate user credentials and receive a JWT token |
| `GET` | `/api/v1/books` | Public | Paginated, searchable, filterable, and cached book catalog |
| `POST` | `/api/v1/books` | `Admin` | Create a new book (validated with FluentValidation + ISBN Value Object) |
| `PATCH` | `/api/v1/books/{id}/stock` | `Admin` | Increase or decrease book inventory via domain guards |
| `POST` | `/api/v1/orders` | Authenticated | Place an order, apply coupon (`CLEAN20`), deduct stock atomically |
| `GET` | `/api/v1/orders/customer/{customerId}` | Authenticated | Retrieve a customer's order history with line items |

## Getting Started

### Prerequisites
* [.NET 9 SDK](https://dotnet.microsoft.com/download)
* SQL Server (LocalDB or SQL Server Express/Developer)

### Run Locally
1. Clone the repository:
   ```bash
   git clone https://github.com/Evz2002/BookVerse.git
   cd BookVerse
   ```
2. Update `DefaultConnection` in `BookVerse/appsettings.json` if needed (default uses `Server=.;Database=BookVerseDb;...`).
3. Apply EF Core migrations to create the database:
   ```bash
   dotnet ef database update --project BookVerse.Infrastructure --startup-project BookVerse
   ```
4. Run unit tests:
   ```bash
   dotnet test
   ```
5. Start the API:
   ```bash
   dotnet run --project BookVerse
   ```
   Open `/swagger` for interactive API documentation or `/` for the AI-generated test storefront.
