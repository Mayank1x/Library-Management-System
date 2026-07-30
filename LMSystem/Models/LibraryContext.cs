using Microsoft.EntityFrameworkCore;

namespace LMSystem.Models
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>().HasData(
                new Book { BookId = 1, Title = "The Pragmatic Programmer", Author = "Andrew Hunt and David Thomas", ISBN = "978-0201616224", PublishedDate = new DateTime(2021, 10, 30), IsAvailable = true },
                new Book { BookId = 2, Title = "Design Pattern using C#", Author = "Robert C. Martin", ISBN = "978-0132350884", PublishedDate = new DateTime(2023, 8, 1), IsAvailable = true },
                new Book { BookId = 3, Title = "Mastering ASP.NET Core", Author = "Pranaya Kumar Rout", ISBN = "978-0451616235", PublishedDate = new DateTime(2022, 11, 22), IsAvailable = true },
                new Book { BookId = 4, Title = "SQL Server with DBA", Author = "Rakesh Kumar", ISBN = "978-4562350123", PublishedDate = new DateTime(2020, 8, 15), IsAvailable = true },
                new Book { BookId = 5, Title = "Clean Architecture", Author = "Robert C. Martin", ISBN = "978-0134494166", PublishedDate = new DateTime(2017, 9, 20), IsAvailable = true },
                new Book { BookId = 6, Title = "Refactoring", Author = "Martin Fowler", ISBN = "978-0134757599", PublishedDate = new DateTime(2018, 11, 30), IsAvailable = true },
                new Book { BookId = 7, Title = "The Clean Coder", Author = "Robert C. Martin", ISBN = "978-0137081073", PublishedDate = new DateTime(2011, 5, 13), IsAvailable = true },
                new Book { BookId = 8, Title = "Domain-Driven Design", Author = "Eric Evans", ISBN = "978-0321125217", PublishedDate = new DateTime(2003, 8, 30), IsAvailable = true },
                new Book { BookId = 9, Title = "You Don't Know JS", Author = "Kyle Simpson", ISBN = "978-1491904244", PublishedDate = new DateTime(2015, 3, 1), IsAvailable = true },
                new Book { BookId = 10, Title = "Introduction to Algorithms", Author = "Thomas H. Cormen", ISBN = "978-0262033848", PublishedDate = new DateTime(2009, 7, 31), IsAvailable = true }
            );
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<BorrowRecord> BorrowRecords { get; set; }
        public DbSet<Publication> Publications { get; set; }
    }
}
