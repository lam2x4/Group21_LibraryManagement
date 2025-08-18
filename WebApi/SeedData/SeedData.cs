using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public static class SeedData
{
    public static void Seed(this ModelBuilder modelBuilder)
    {
        // Seeding dữ liệu cho các bảng
        // Seed Authors
        modelBuilder.Entity<Author>().HasData(
            new Author { AuthorId = 1, FirstName = "Nguyễn Nhật", LastName = "Ánh", Bio = "Tác giả nổi tiếng với các tác phẩm văn học thiếu nhi." },
            new Author { AuthorId = 2, FirstName = "Dale", LastName = "Carnegie", Bio = "Nhà văn, diễn giả người Mỹ." },
            new Author { AuthorId = 3, FirstName = "Adam", LastName = "Khoo", Bio = "Chuyên gia đào tạo, diễn giả người Singapore." }
        );

        // Seed Publishers
        modelBuilder.Entity<Publisher>().HasData(
            new Publisher { PublisherId = 1, Name = "Nhà xuất bản Kim Đồng", Address = "Hà Nội" },
            new Publisher { PublisherId = 2, Name = "Nhà xuất bản Trẻ", Address = "TP. Hồ Chí Minh" }
        );

        // Seed Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { CategoryId = 1, Name = "Thiếu nhi" },
            new Category { CategoryId = 2, Name = "Kỹ năng sống" },
            new Category { CategoryId = 3, Name = "Tâm lý học" }
        );

        // Seed Books
        modelBuilder.Entity<Book>().HasData(
            new Book { BookId = 1, Title = "Tôi thấy hoa vàng trên cỏ xanh", PublisherId = 1, ISBN13 = "9786042048564", PublicationYear = 2010, Description = "Câu chuyện về tuổi thơ hồn nhiên, trong trẻo ở vùng quê.", ImageUrl = "https://example.com/hoa-vang.jpg" },
            new Book { BookId = 2, Title = "Đắc nhân tâm", PublisherId = 2, ISBN13 = "9786046473130", PublicationYear = 2012, Description = "Tuyệt tác về nghệ thuật đối nhân xử thế.", ImageUrl = "https://example.com/dac-nhan-tam.jpg" },
            new Book { BookId = 3, Title = "Tôi tài giỏi, bạn cũng thế", PublisherId = 2, ISBN13 = "9786046473147", PublicationYear = 2015, Description = "Hướng dẫn phương pháp học tập hiệu quả.", ImageUrl = "https://example.com/toi-tai-gioi.jpg" }
        );

        // Seed BookAuthors
        modelBuilder.Entity<BookAuthor>().HasData(
            new BookAuthor { BookId = 1, AuthorId = 1 },
            new BookAuthor { BookId = 2, AuthorId = 2 },
            new BookAuthor { BookId = 3, AuthorId = 3 }
        );

        // Seed BookCategories
        modelBuilder.Entity<BookCategory>().HasData(
            new BookCategory { BookId = 1, CategoryId = 1 },
            new BookCategory { BookId = 2, CategoryId = 2 },
            new BookCategory { BookId = 2, CategoryId = 3 },
            new BookCategory { BookId = 3, CategoryId = 2 }
        );

        // Seed BookItems
        modelBuilder.Entity<BookItem>().HasData(
            new BookItem { ItemId = 1, BookId = 1, Barcode = "BK-0001-A", Status = "Available", ShelfLocation = "A1-01" },
            new BookItem { ItemId = 2, BookId = 1, Barcode = "BK-0001-B", Status = "Available", ShelfLocation = "A1-01" },
            new BookItem { ItemId = 3, BookId = 1, Barcode = "BK-0001-C", Status = "Available", ShelfLocation = "A1-01" },
            new BookItem { ItemId = 4, BookId = 2, Barcode = "BK-0002-A", Status = "Available", ShelfLocation = "B2-02" },
            new BookItem { ItemId = 5, BookId = 2, Barcode = "BK-0002-B", Status = "Available", ShelfLocation = "B2-02" },
            new BookItem { ItemId = 6, BookId = 2, Barcode = "BK-0002-C", Status = "Available", ShelfLocation = "B2-02" },
            new BookItem { ItemId = 7, BookId = 3, Barcode = "BK-0003-A", Status = "Available", ShelfLocation = "C3-03" },
            new BookItem { ItemId = 8, BookId = 3, Barcode = "BK-0003-B", Status = "Available", ShelfLocation = "C3-03" },
            new BookItem { ItemId = 9, BookId = 3, Barcode = "BK-0003-C", Status = "Available", ShelfLocation = "C3-03" }
        );

        // Seed Users, Roles và UserRoles
        var hasher = new PasswordHasher<ApplicationUser>();

        var adminUser = new ApplicationUser
        {
            Id = "admin-1",
            UserName = "admin",
            NormalizedUserName = "ADMIN",
            Email = "admin@example.com",
            NormalizedEmail = "ADMIN@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString()
        };
        adminUser.PasswordHash = hasher.HashPassword(adminUser, "P@ssword123");

        var librarianUser = new ApplicationUser
        {
            Id = "librarian-1",
            UserName = "librarian",
            NormalizedUserName = "LIBRARIAN",
            Email = "librarian@example.com",
            NormalizedEmail = "LIBRARIAN@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString()
        };
        librarianUser.PasswordHash = hasher.HashPassword(librarianUser, "P@ssword123");

        var regularUser = new ApplicationUser
        {
            Id = "user-1",
            UserName = "user",
            NormalizedUserName = "USER",
            Email = "user@example.com",
            NormalizedEmail = "USER@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString()
        };
        regularUser.PasswordHash = hasher.HashPassword(regularUser, "P@ssword123");

        modelBuilder.Entity<ApplicationUser>().HasData(adminUser, librarianUser, regularUser);

        var adminRole = new IdentityRole { Id = "admin-role", Name = "Admin", NormalizedName = "ADMIN" };
        var librarianRole = new IdentityRole { Id = "librarian-role", Name = "Librarian", NormalizedName = "LIBRARIAN" };
        var userRole = new IdentityRole { Id = "user-role", Name = "User", NormalizedName = "USER" };

        modelBuilder.Entity<IdentityRole>().HasData(adminRole, librarianRole, userRole);

        modelBuilder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string> { RoleId = "admin-role", UserId = "admin-1" },
            new IdentityUserRole<string> { RoleId = "librarian-role", UserId = "librarian-1" },
            new IdentityUserRole<string> { RoleId = "user-role", UserId = "user-1" }
        );
    }
}