using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

public static class SeedData
{
    public static void Seed(this ModelBuilder modelBuilder)
    {
        var hasher = new PasswordHasher<ApplicationUser>();

        var user1 = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "user1",
            NormalizedUserName = "USER1",
            Email = "user1@example.com",
            NormalizedEmail = "USER1@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString("D"),
            ConcurrencyStamp = Guid.NewGuid().ToString("D"),
            PasswordHash = hasher.HashPassword(null, "Password@123")
        };

        var user2 = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "user2",
            NormalizedUserName = "USER2",
            Email = "user2@example.com",
            NormalizedEmail = "USER2@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString("D"),
            ConcurrencyStamp = Guid.NewGuid().ToString("D"),
            PasswordHash = hasher.HashPassword(null, "Password@123")
        };

        var user3 = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "librarian",
            NormalizedUserName = "LIBRARIAN",
            Email = "librarian@example.com",
            NormalizedEmail = "LIBRARIAN@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString("D"),
            ConcurrencyStamp = Guid.NewGuid().ToString("D"),
            PasswordHash = hasher.HashPassword(null, "Password@123")
        };

        modelBuilder.Entity<ApplicationUser>().HasData(user1, user2, user3);
    }
}
