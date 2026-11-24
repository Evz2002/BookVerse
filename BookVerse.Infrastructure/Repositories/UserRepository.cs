namespace BookVerse.Infrastructure.Repositories;
using BookVerse.Domain.Entities;
using BookVerse.Domain.Repositories;
using BookVerse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AnyAsync(u => u.Email == email, cancellationToken);
    }
    public void Add(User user)
    {
        _dbContext.Users.Add(user);
    }
}