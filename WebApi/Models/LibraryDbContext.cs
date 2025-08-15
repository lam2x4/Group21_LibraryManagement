using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class LibraryDbContext : IdentityDbContext<ApplicationUser>
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

    public DbSet<Author> Authors { get; set; }
    public DbSet<Publisher> Publishers { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<BookItem> BookItems { get; set; }
    public DbSet<Loan> Loans { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Fine> Fines { get; set; }
    public DbSet<BookAuthor> BookAuthors { get; set; }
    public DbSet<BookCategory> BookCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Gọi phương thức OnModelCreating của lớp cha để Identity hoạt động đúng
        base.OnModelCreating(modelBuilder);

        // --- Cấu hình Fluent API ---

        // Bảng Author
        modelBuilder.Entity<Author>(entity =>
        {
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Bio).HasMaxLength(500);
        });

        // Bảng Publisher
        modelBuilder.Entity<Publisher>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(255);
        });

        // Bảng Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
        });

        // Bảng Book
        modelBuilder.Entity<Book>(entity =>
        {
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ISBN13).IsRequired().HasMaxLength(13);
            entity.HasIndex(e => e.ISBN13).IsUnique();
            entity.Property(e => e.Description).HasMaxLength(4000);
            entity.Property(e => e.ImageUrl).HasMaxLength(2000);
        });

        // Bảng BookItem
        modelBuilder.Entity<BookItem>(entity =>
        {
            entity.Property(e => e.Barcode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ShelfLocation).HasMaxLength(50);
        });

        // Bảng Loan
        modelBuilder.Entity<Loan>(entity =>
        {
            entity.Property(e => e.LoanDate).IsRequired();
            entity.Property(e => e.DueDate).IsRequired();
            entity.Property(e => e.IsReturned).IsRequired();

            // Khắc phục lỗi "multiple cascade paths" bằng cách thiết lập
            // ON DELETE NO ACTION cho các khóa ngoại trỏ đến AspNetUsers
            entity.HasOne(l => l.User)
                  .WithMany()
                  .HasForeignKey(l => l.UserId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(l => l.Librarian)
                  .WithMany()
                  .HasForeignKey(l => l.LibrarianId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // Bảng Reservation
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.Property(e => e.ReservationDate).IsRequired();
            entity.Property(e => e.ExpirationDate).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
        });

        // Bảng Fine
        modelBuilder.Entity<Fine>(entity =>
        {
            entity.Property(e => e.Amount).IsRequired().HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PaymentStatus).IsRequired().HasMaxLength(20);

            // Mối quan hệ 1-1 với Loan
            entity.HasOne(f => f.Loan)
                  .WithOne(l => l.Fine)
                  .HasForeignKey<Fine>(f => f.LoanId);
        });

        // Bảng liên kết nhiều-nhiều
        modelBuilder.Entity<BookAuthor>().HasKey(ba => new { ba.BookId, ba.AuthorId });
        modelBuilder.Entity<BookCategory>().HasKey(bc => new { bc.BookId, bc.CategoryId });


        // --- Seeding Data ---
        modelBuilder.Entity<Author>().HasData(
            new Author { AuthorId = 1, FirstName = "Nguyễn Nhật", LastName = "Ánh", Bio = "Tác giả nổi tiếng với các tác phẩm văn học thiếu nhi." },
            new Author { AuthorId = 2, FirstName = "Dale", LastName = "Carnegie", Bio = "Nhà văn, diễn giả người Mỹ." },
            new Author { AuthorId = 3, FirstName = "Adam", LastName = "Khoo", Bio = "Chuyên gia đào tạo, diễn giả người Singapore." }
        );

        modelBuilder.Entity<Publisher>().HasData(
            new Publisher { PublisherId = 1, Name = "Nhà xuất bản Kim Đồng", Address = "Hà Nội" },
            new Publisher { PublisherId = 2, Name = "Nhà xuất bản Trẻ", Address = "TP. Hồ Chí Minh" }
        );

        modelBuilder.Entity<Category>().HasData(
            new Category { CategoryId = 1, Name = "Thiếu nhi" },
            new Category { CategoryId = 2, Name = "Kỹ năng sống" },
            new Category { CategoryId = 3, Name = "Tâm lý học" }
        );

        modelBuilder.Entity<Book>().HasData(
            new Book { BookId = 1, Title = "Tôi thấy hoa vàng trên cỏ xanh", PublisherId = 1, ISBN13 = "9786042048564", PublicationYear = 2010, Description = "Câu chuyện về tuổi thơ hồn nhiên, trong trẻo ở vùng quê.", ImageUrl = "https://example.com/hoa-vang.jpg" },
            new Book { BookId = 2, Title = "Đắc nhân tâm", PublisherId = 2, ISBN13 = "9786046473130", PublicationYear = 2012, Description = "Tuyệt tác về nghệ thuật đối nhân xử thế.", ImageUrl = "https://example.com/dac-nhan-tam.jpg" },
            new Book { BookId = 3, Title = "Tôi tài giỏi, bạn cũng thế", PublisherId = 2, ISBN13 = "9786046473147", PublicationYear = 2015, Description = "Hướng dẫn phương pháp học tập hiệu quả.", ImageUrl = "https://example.com/toi-tai-gioi.jpg" }
        );

        modelBuilder.Entity<BookAuthor>().HasData(
            new BookAuthor { BookId = 1, AuthorId = 1 },
            new BookAuthor { BookId = 2, AuthorId = 2 },
            new BookAuthor { BookId = 3, AuthorId = 3 }
        );

        modelBuilder.Entity<BookCategory>().HasData(
            new BookCategory { BookId = 1, CategoryId = 1 },
            new BookCategory { BookId = 2, CategoryId = 2 },
            new BookCategory { BookId = 2, CategoryId = 3 },
            new BookCategory { BookId = 3, CategoryId = 2 }
        );

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
    }
}