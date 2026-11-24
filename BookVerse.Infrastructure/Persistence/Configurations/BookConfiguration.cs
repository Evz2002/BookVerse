namespace BookVerse.Infrastructure.Persistence.Configurations;
using BookVerse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Author)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(b => b.Isbn)
            .IsRequired()
            .HasMaxLength(13)
            .IsUnicode(false); 

        builder.HasIndex(b => b.Isbn)
            .IsUnique();

        builder.Property(b => b.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.Price)
            .HasPrecision(18, 0); 

        builder.Property(b => b.RowVersion)
            .IsRowVersion();

        builder.HasQueryFilter(b => b.IsActive);
    }
}