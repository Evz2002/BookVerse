namespace BookVerse.Application.DTOs;
public sealed record RegisterRequest(
    string FullName,
    string Email,
    string Password,
    string Role = "Customer");
public sealed record LoginRequest(
    string Email,
    string Password);
public sealed record AuthResponseDto(
    Guid UserId,
    string FullName,
    string Email,
    string Role,
    string AccessToken);