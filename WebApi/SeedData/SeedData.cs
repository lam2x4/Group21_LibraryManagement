using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;

public static class SeedData
{
    public static void Seed(this ModelBuilder modelBuilder)
    {
        // Seeding dữ liệu cho các bảng hiện có
        // Seed Authors
        modelBuilder.Entity<Author>().HasData(
            new Author { AuthorId = 1, FirstName = "Nguyễn Nhật", LastName = "Ánh", Bio = "Tác giả nổi tiếng với các tác phẩm văn học thiếu nhi." },
            new Author { AuthorId = 2, FirstName = "Dale", LastName = "Carnegie", Bio = "Nhà văn, diễn giả người Mỹ." },
            new Author { AuthorId = 3, FirstName = "Adam", LastName = "Khoo", Bio = "Chuyên gia đào tạo, diễn giả người Singapore." },
            new Author { AuthorId = 4, FirstName = "Haruki", LastName = "Murakami", Bio = "Nhà văn Nhật Bản nổi tiếng với phong cách siêu thực." },
            new Author { AuthorId = 5, FirstName = "J.K.", LastName = "Rowling", Bio = "Tác giả bộ truyện Harry Potter." },
            new Author { AuthorId = 6, FirstName = "Stephen", LastName = "King", Bio = "Ông hoàng truyện kinh dị." }
        );

        // Seed Publishers
        modelBuilder.Entity<Publisher>().HasData(
            new Publisher { PublisherId = 1, Name = "Nhà xuất bản Kim Đồng", Address = "Hà Nội" },
            new Publisher { PublisherId = 2, Name = "Nhà xuất bản Trẻ", Address = "TP. Hồ Chí Minh" },
            new Publisher { PublisherId = 3, Name = "Nhã Nam", Address = "Hà Nội" },
            new Publisher { PublisherId = 4, Name = "Penguin Books", Address = "New York, USA" }
        );

        // Seed Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { CategoryId = 1, Name = "Thiếu nhi" },
            new Category { CategoryId = 2, Name = "Kỹ năng sống" },
            new Category { CategoryId = 3, Name = "Tâm lý học" },
            new Category { CategoryId = 4, Name = "Tiểu thuyết" },
            new Category { CategoryId = 5, Name = "Kinh dị" },
            new Category { CategoryId = 6, Name = "Khoa học viễn tưởng" },
            new Category { CategoryId = 7, Name = "Lịch sử" }
        );

        // Seed Books 
        modelBuilder.Entity<Book>().HasData(
            new Book { BookId = 1, Title = "Tôi thấy hoa vàng trên cỏ xanh", PublisherId = 1, ISBN13 = "9786042048564", PublicationYear = 2010, Description = "Câu chuyện về tuổi thơ hồn nhiên, trong trẻo ở vùng quê.", ImageUrl = "https://example.com/hoa-vang.jpg" },
            new Book { BookId = 2, Title = "Đắc nhân tâm", PublisherId = 2, ISBN13 = "9786046473130", PublicationYear = 2012, Description = "Tuyệt tác về nghệ thuật đối nhân xử thế.", ImageUrl = "https://example.com/dac-nhan-tam.jpg" },
            new Book { BookId = 3, Title = "Tôi tài giỏi, bạn cũng thế", PublisherId = 2, ISBN13 = "9786046473147", PublicationYear = 2015, Description = "Hướng dẫn phương pháp học tập hiệu quả.", ImageUrl = "https://example.com/toi-tai-gioi.jpg" },
            new Book { BookId = 4, Title = "Rừng Na Uy", PublisherId = 3, ISBN13 = "9786049581907", PublicationYear = 2014, Description = "Tác phẩm tiêu biểu của Haruki Murakami.", ImageUrl = "https://example.com/rung-na-uy.jpg" },
            new Book { BookId = 5, Title = "Harry Potter và Hòn đá Phù thủy", PublisherId = 4, ISBN13 = "9780747532743", PublicationYear = 1997, Description = "Cuốn sách đầu tiên trong series Harry Potter.", ImageUrl = "https://example.com/harry-potter-1.jpg" },
            new Book { BookId = 6, Title = "IT", PublisherId = 4, ISBN13 = "9780451419708", PublicationYear = 1986, Description = "Câu chuyện kinh dị về một thực thể bí ẩn.", ImageUrl = "https://example.com/it.jpg" },
            new Book { BookId = 7, Title = "Nhà giả kim", PublisherId = 3, ISBN13 = "9786049581914", PublicationYear = 2010, Description = "Cuộc hành trình của cậu bé chăn cừu đi tìm kho báu.", ImageUrl = "https://example.com/nha-gia-kim.jpg" },
            new Book { BookId = 8, Title = "Bảy viên ngọc rồng", PublisherId = 1, ISBN13 = "9786042079087", PublicationYear = 2011, Description = "Bộ truyện tranh kinh điển của Nhật Bản.", ImageUrl = "https://example.com/bay-vien-ngoc-rong.jpg" },
            new Book { BookId = 9, Title = "Lược sử loài người", PublisherId = 3, ISBN13 = "9786049581921", PublicationYear = 2013, Description = "Từ vượn người tới người tinh khôn.", ImageUrl = "https://example.com/luoc-su-loai-nguoi.jpg" },
            new Book { BookId = 10, Title = "Tôi là Bê-Tô", PublisherId = 1, ISBN13 = "9786042079094", PublicationYear = 2015, Description = "Câu chuyện cảm động về một chú chó.", ImageUrl = "https://example.com/toi-la-beto.jpg" },
            new Book { BookId = 11, Title = "Thám tử lừng danh Conan", PublisherId = 1, ISBN13 = "9786042079100", PublicationYear = 2018, Description = "Series truyện trinh thám nổi tiếng.", ImageUrl = "https://example.com/conan.jpg" },
            new Book { BookId = 12, Title = "Muôn kiếp nhân sinh", PublisherId = 2, ISBN13 = "9786042079117", PublicationYear = 2020, Description = "Câu chuyện về luật nhân quả.", ImageUrl = "https://example.com/muon-kiep-nhan-sinh.jpg" },
            new Book { BookId = 13, Title = "Nhóc Miko!", PublisherId = 1, ISBN13 = "9786042079124", PublicationYear = 2019, Description = "Truyện tranh hài hước về một cô bé tiểu học.", ImageUrl = "https://example.com/nhoc-miko.jpg" },
            new Book { BookId = 14, Title = "Phía sau nghi can X", PublisherId = 3, ISBN13 = "9786049581938", PublicationYear = 2015, Description = "Tiểu thuyết trinh thám của Keigo Higashino.", ImageUrl = "https://example.com/phai-sau-nghi-can-x.jpg" },
            new Book { BookId = 15, Title = "Sống", PublisherId = 3, ISBN13 = "9786049581945", PublicationYear = 2011, Description = "Cuốn sách về ý nghĩa của cuộc sống.", ImageUrl = "https://example.com/song.jpg" },
            new Book { BookId = 16, Title = "Đọc vị bất kỳ ai", PublisherId = 2, ISBN13 = "9786046473154", PublicationYear = 2016, Description = "Sách về tâm lý học và cách hiểu người khác.", ImageUrl = "https://example.com/doc-vi.jpg" },
            new Book { BookId = 17, Title = "Thần thoại Hy Lạp", PublisherId = 4, ISBN13 = "9780141380922", PublicationYear = 2007, Description = "Tập hợp các câu chuyện thần thoại Hy Lạp.", ImageUrl = "https://example.com/than-thoai-hy-lap.jpg" },
            new Book { BookId = 18, Title = "Số đỏ", PublisherId = 3, ISBN13 = "9786049581952", PublicationYear = 2016, Description = "Tiểu thuyết châm biếm của Vũ Trọng Phụng.", ImageUrl = "https://example.com/so-do.jpg" },
            new Book { BookId = 19, Title = "Kiếp nào ta cũng tìm thấy nhau", PublisherId = 2, ISBN13 = "9786046473161", PublicationYear = 2019, Description = "Tác phẩm của Brian Weiss về tiền kiếp.", ImageUrl = "https://example.com/kiep-nao.jpg" },
            new Book { BookId = 20, Title = "Hoàng tử bé", PublisherId = 1, ISBN13 = "9786042079131", PublicationYear = 2018, Description = "Câu chuyện triết lý về tình yêu và cuộc sống.", ImageUrl = "https://example.com/hoang-tu-be.jpg" }
        );

        // Seed BookAuthors (liên kết thêm các tác giả và sách mới)
        modelBuilder.Entity<BookAuthor>().HasData(
            new BookAuthor { BookId = 1, AuthorId = 1 },
            new BookAuthor { BookId = 2, AuthorId = 2 },
            new BookAuthor { BookId = 3, AuthorId = 3 },
            new BookAuthor { BookId = 4, AuthorId = 4 },
            new BookAuthor { BookId = 5, AuthorId = 5 },
            new BookAuthor { BookId = 6, AuthorId = 6 },
            new BookAuthor { BookId = 7, AuthorId = 2 },
            new BookAuthor { BookId = 8, AuthorId = 1 },
            new BookAuthor { BookId = 9, AuthorId = 3 },
            new BookAuthor { BookId = 10, AuthorId = 1 },
            new BookAuthor { BookId = 11, AuthorId = 4 },
            new BookAuthor { BookId = 12, AuthorId = 2 },
            new BookAuthor { BookId = 13, AuthorId = 1 },
            new BookAuthor { BookId = 14, AuthorId = 3 },
            new BookAuthor { BookId = 15, AuthorId = 2 },
            new BookAuthor { BookId = 16, AuthorId = 2 },
            new BookAuthor { BookId = 17, AuthorId = 6 },
            new BookAuthor { BookId = 18, AuthorId = 1 },
            new BookAuthor { BookId = 19, AuthorId = 5 },
            new BookAuthor { BookId = 20, AuthorId = 1 }
        );

        // Seed BookCategories (liên kết thêm các thể loại và sách mới)
        modelBuilder.Entity<BookCategory>().HasData(
            new BookCategory { BookId = 1, CategoryId = 1 },
            new BookCategory { BookId = 2, CategoryId = 2 },
            new BookCategory { BookId = 2, CategoryId = 3 },
            new BookCategory { BookId = 3, CategoryId = 2 },
            new BookCategory { BookId = 4, CategoryId = 4 },
            new BookCategory { BookId = 5, CategoryId = 6 },
            new BookCategory { BookId = 6, CategoryId = 5 },
            new BookCategory { BookId = 7, CategoryId = 2 },
            new BookCategory { BookId = 8, CategoryId = 1 },
            new BookCategory { BookId = 9, CategoryId = 7 },
            new BookCategory { BookId = 10, CategoryId = 1 },
            new BookCategory { BookId = 11, CategoryId = 4 },
            new BookCategory { BookId = 12, CategoryId = 3 },
            new BookCategory { BookId = 13, CategoryId = 1 },
            new BookCategory { BookId = 14, CategoryId = 4 },
            new BookCategory { BookId = 15, CategoryId = 2 },
            new BookCategory { BookId = 16, CategoryId = 3 },
            new BookCategory { BookId = 17, CategoryId = 7 },
            new BookCategory { BookId = 18, CategoryId = 4 },
            new BookCategory { BookId = 19, CategoryId = 3 },
            new BookCategory { BookId = 20, CategoryId = 1 }
        );

        // Seed BookItems (20 cuốn sách mới, mỗi sách có 3 bản)
        for (int i = 1; i <= 20; i++)
        {
            modelBuilder.Entity<BookItem>().HasData(
                new BookItem { ItemId = (i - 1) * 3 + 1, BookId = i, Barcode = $"BK-{i:D4}-A", Status = "Available", ShelfLocation = $"Shelf-{i}-A" },
                new BookItem { ItemId = (i - 1) * 3 + 2, BookId = i, Barcode = $"BK-{i:D4}-B", Status = "Available", ShelfLocation = $"Shelf-{i}-B" },
                new BookItem { ItemId = (i - 1) * 3 + 3, BookId = i, Barcode = $"BK-{i:D4}-C", Status = "Available", ShelfLocation = $"Shelf-{i}-C" }
            );
        }

        // --- Seeding dữ liệu cho Room và BookingRoom ---

        // Seed Rooms (5 phòng mẫu)
        modelBuilder.Entity<Room>().HasData(
            new Room { RoomId = 1, RoomName = "Phòng họp nhỏ", RoomDescription = "Phòng họp cho 4-6 người, có màn hình chiếu.", PricePerNight = 50.00m, IsAvailable = true },
            new Room { RoomId = 2, RoomName = "Phòng họp lớn", RoomDescription = "Phòng họp cho 10-15 người, có bảng trắng.", PricePerNight = 100.00m, IsAvailable = true },
            new Room { RoomId = 3, RoomName = "Phòng nghiên cứu 1", RoomDescription = "Phòng yên tĩnh, dành cho cá nhân nghiên cứu.", PricePerNight = 30.00m, IsAvailable = true },
            new Room { RoomId = 4, RoomName = "Phòng nghiên cứu 2", RoomDescription = "Phòng yên tĩnh, dành cho cá nhân nghiên cứu.", PricePerNight = 30.00m, IsAvailable = true },
            new Room { RoomId = 5, RoomName = "Phòng đa năng", RoomDescription = "Không gian linh hoạt, có thể tổ chức workshop.", PricePerNight = 80.00m, IsAvailable = true }
        );

        // Seed BookingRooms 
        modelBuilder.Entity<BookingRoom>().HasData(
            new BookingRoom
            {
                BookingId = 1,
                RoomId = 1,
                UserId = "user-1", // Thêm UserId
                CheckInDate = new DateTime(2025, 8, 25),
                CheckOutDate = new DateTime(2025, 8, 26),
                Status = BookingStatus.Pending
            },
            new BookingRoom
            {
                BookingId = 2,
                RoomId = 2,
                UserId = "user-1", // Thêm UserId
                CheckInDate = new DateTime(2025, 8, 25),
                CheckOutDate = new DateTime(2025, 8, 26),
                Status = BookingStatus.Confirmed
            },
            new BookingRoom
            {
                BookingId = 3,
                RoomId = 3,
                UserId = "user-1", // Thêm UserId
                CheckInDate = new DateTime(2025, 8, 26),
                CheckOutDate = new DateTime(2025, 8, 27),
                Status = BookingStatus.Pending
            },
            new BookingRoom
            {
                BookingId = 4,
                RoomId = 1,
                UserId = "user-1", // Thêm UserId
                CheckInDate = new DateTime(2025, 9, 10),
                CheckOutDate = new DateTime(2025, 9, 11),
                Status = BookingStatus.Confirmed
            },
            new BookingRoom
            {
                BookingId = 5,
                RoomId = 4,
                UserId = "user-1", // Thêm UserId
                CheckInDate = new DateTime(2025, 9, 15),
                CheckOutDate = new DateTime(2025, 9, 16),
                Status = BookingStatus.Cancelled
            }
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