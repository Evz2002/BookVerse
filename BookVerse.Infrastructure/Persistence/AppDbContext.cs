namespace BookVerse.Infrastructure.Persistence;

using BookVerse.Domain.Entities;
using BookVerse.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

public sealed class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // به جای معرفی دستی تک‌تک کلاس‌های Configuration، کل این Assembly را خودکار اسکن و اعمال می‌کند
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}