namespace BookVerse.Application.Services;
using BookVerse.Application.Abstractions;
using BookVerse.Application.DTOs;
using BookVerse.Domain.Common;
using BookVerse.Domain.Entities;
using BookVerse.Domain.Repositories;
using Microsoft.Extensions.Logging;
public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponseDto>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<AuthService> _logger;
    public AuthService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }
    public async Task<Result<AuthResponseDto>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (await _userRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken))
        {
            _logger.LogWarning("Registration rejected: Duplicate email {Email}", normalizedEmail);
            return Result.Failure<AuthResponseDto>(UserErrors.DuplicateEmail);
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            return Result.Failure<AuthResponseDto>(new Error("User.WeakPassword", "رمز عبور باید حداقل ۶ کاراکتر باشد."));

        var passwordHash = _passwordHasher.Hash(request.Password);
        var userResult = User.Create(request.FullName, normalizedEmail, passwordHash, request.Role);
        if (userResult.IsFailure)
            return Result.Failure<AuthResponseDto>(userResult.Error);

        var user = userResult.Value;
        _userRepository.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenGenerator.GenerateToken(user);
        _logger.LogInformation("New user {UserId} registered with role {Role}", user.Id, user.Role);

        return Result.Success(new AuthResponseDto(user.Id, user.FullName, user.Email, user.Role, token));
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for email {Email}", normalizedEmail);
            return Result.Failure<AuthResponseDto>(UserErrors.InvalidCredentials);
        }

        var token = _jwtTokenGenerator.GenerateToken(user);
        _logger.LogInformation("User {UserId} logged in successfully", user.Id);

        return Result.Success(new AuthResponseDto(user.Id, user.FullName, user.Email, user.Role, token));
    }
}