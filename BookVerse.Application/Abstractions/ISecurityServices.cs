namespace BookVerse.Application.Abstractions;
using BookVerse.Domain.Entities;
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}
public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}