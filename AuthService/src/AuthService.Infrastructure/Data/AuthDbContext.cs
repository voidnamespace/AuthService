using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Ignore(u => u.Email);

            entity.Property(u => u.Email)
                  .HasConversion(
                     email => email != null ? email._email : "",
                     str => new EmailVO(str))
                  .HasColumnName("Email")
                  .HasMaxLength(255)
                  .IsRequired();

            entity.HasIndex(u => u.Email)
                .IsUnique();

            entity.Property(u => u.PasswordHash)
                .HasConversion(
                    pwd => pwd != null ? pwd.Hash : "",
                    hash => PasswordVO.FromHash(hash))
                .HasColumnName("PasswordHash")
                .IsRequired();

            entity.Property(u => u.Role)
                .HasConversion<string>()
                .HasDefaultValue(Roles.Customer)
                .IsRequired();

            entity.Property(u => u.CreatedAt).IsRequired();
            entity.Property(u => u.IsActive).IsRequired().HasDefaultValue(true);

            entity.HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Token)
                .IsRequired()
                .HasMaxLength(500);

            entity.HasIndex(e => e.Token)
                .IsUnique();

            entity.Property(e => e.ExpiryDate)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.IsRevoked)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.UserId)
                .IsRequired();
        });
    }
}
