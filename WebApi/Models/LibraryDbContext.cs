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

            // Quan hệ Loan.User → ApplicationUser
            entity.HasOne(l => l.User)
                  .WithMany(u => u.LoansAsUser)   // navigation ngược
                  .HasForeignKey(l => l.UserId)
                  .OnDelete(DeleteBehavior.NoAction);

            // Quan hệ Loan.Librarian → ApplicationUser
            entity.HasOne(l => l.Librarian)
                  .WithMany(u => u.LoansAsLibrarian) // navigation ngược
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

        modelBuilder.Seed();
    }
}