# Library Management System — Full Build Specification

> **Instructions for the coding agent:** Build this exact ASP.NET Core MVC project, in the order given below, using the exact code provided in each section. Do not rename models, DbSets, namespaces, or routes — the naming below has already been made consistent across all modules. Where a note says "Fix applied," that means the original source material had a bug; the code shown here is the corrected version — use it as-is.
>
> This spec covers **Modules 1–6**: Books, Borrow/Return, Login, Dashboard, Students, Librarians. It comes from course sessions 1–3 of a multi-session tutorial. Sessions 4–5 exist but are not yet available — search/pagination for Books, Librarians, and Students was explicitly flagged as "future scope" and is **out of scope for this build**.

---

## 1. Tech Stack

- **.NET 8**, ASP.NET Core MVC
- **Entity Framework Core 8.0.0** (Code-First) — used only for the Books/Borrow module
- **SQL Server** (LocalDB) — database name `LMS`
- **ADO.NET** (`Microsoft.Data.SqlClient`) — used directly (no EF) for Students, Librarians, Login, Dashboard
- **Bootstrap 5.3.0** (CDN) + **Bootstrap Icons 1.10.5** (CDN)
- Data Annotations for validation

**NuGet packages to install:**
```
Install-Package Microsoft.EntityFrameworkCore.SqlServer -Version 8.0.0
Install-Package Microsoft.EntityFrameworkCore.Tools -Version 8.0.0
Install-Package Microsoft.Data.SqlClient
```

---

## 2. Project Setup

1. Create a new **ASP.NET Core Web App (Model-View-Controller)** project named **`LMSystem`**, targeting .NET 8. Keep the default scaffolded files (`HomeController`, `Views/Home/*`, `Views/Shared/Error.cshtml`, `Views/_ViewImports.cshtml`, `Views/_ViewStart.cshtml`, `wwwroot/lib/*`) — they are referenced later and don't need changes, other than what's listed below.
2. Install the NuGet packages above.
3. Edit `Views/_ViewImports.cshtml` to read exactly:

```csharp
@using LMSystem
@using LMSystem.Models
@using LMSystem.ViewModels
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

4. Folder structure to create:

```
LMSystem/
├── Models/
│   ├── Book.cs
│   ├── BorrowRecord.cs
│   ├── LibraryContext.cs
│   ├── LoginModel.cs
│   ├── DashboardModel.cs
│   ├── LibrarianModel.cs
│   └── StudentModel.cs
├── ViewModels/
│   ├── BorrowViewModel.cs
│   └── ReturnViewModel.cs
├── Controllers/
│   ├── BooksController.cs
│   ├── BorrowController.cs
│   ├── LoginController.cs
│   ├── DashboardController.cs
│   ├── StudentController.cs
│   └── LibrarianController.cs
├── Views/
│   ├── Books/ (Index, Details, Create, Edit, Delete)
│   ├── Borrow/ (Create, Return)
│   ├── Login/ (Index)
│   ├── Dashboard/ (Index)
│   ├── Student/ (Index, Create, Edit)
│   ├── Librarian/ (Index, Create, Edit)
│   └── Shared/ (_Layout, NotFound, NotAvailable, AlreadyReturned — plus existing Error.cshtml)
```

---

## 3. appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=LMS;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

## 4. Models

### Models/Book.cs
```csharp
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace LMSystem.Models
{
    public class Book
    {
        [BindNever]
        public int BookId { get; set; } // PK, never bound from user input

        [Required(ErrorMessage = "The Title field is required.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "The Author field is required.")]
        [StringLength(100, ErrorMessage = "Author name cannot exceed 100 characters.")]
        public string? Author { get; set; }

        [Required(ErrorMessage = "The ISBN field is required.")]
        [RegularExpression(@"^\d{3}-\d{10}$", ErrorMessage = "ISBN must be in the format XXX-XXXXXXXXXX.")]
        public string? ISBN { get; set; }

        [Required(ErrorMessage = "The Published Date field is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Published Date")]
        public DateTime PublishedDate { get; set; }

        [BindNever]
        [Display(Name = "Available")]
        public bool IsAvailable { get; set; } = true;

        [BindNever]
        public ICollection<BorrowRecord>? BorrowRecords { get; set; }
    }
}
```
`[BindNever]` on `BookId` and `IsAvailable` prevents over-posting attacks — a malicious form submission can't set these fields; only server logic can.

### Models/BorrowRecord.cs
```csharp
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace LMSystem.Models
{
    public class BorrowRecord
    {
        [Key]
        public int BorrowRecordId { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required(ErrorMessage = "Please enter Borrower Name")]
        public string? BorrowerName { get; set; }

        [Required(ErrorMessage = "Please enter Borrower Email Address")]
        [EmailAddress(ErrorMessage = "Please enter a valid Email Address")]
        public string? BorrowerEmail { get; set; }

        [Required(ErrorMessage = "Please enter Borrower Phone Number")]
        [Phone(ErrorMessage = "Please enter a Valid Phone Number")]
        public string? Phone { get; set; }

        [BindNever]
        [DataType(DataType.DateTime)]
        public DateTime BorrowDate { get; set; } = DateTime.UtcNow;

        [DataType(DataType.DateTime)]
        public DateTime? ReturnDate { get; set; }

        [BindNever]
        public Book? Book { get; set; }
    }
}
```

### Models/LibraryContext.cs
```csharp
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
                new Book { BookId = 4, Title = "SQL Server with DBA", Author = "Rakesh Kumar", ISBN = "978-4562350123", PublishedDate = new DateTime(2020, 8, 15), IsAvailable = true }
            );
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<BorrowRecord> BorrowRecords { get; set; }
    }
}
```
> **Fix applied:** the source transcripts renamed these DbSets `Books12`, then `Books13` between sessions (dev iteration artifacts). Use the clean names `Books` / `BorrowRecords` — the Dashboard module's raw SQL (`SELECT COUNT(*) FROM Books`) depends on the table actually being named `Books`.

### Models/LoginModel.cs
```csharp
namespace LMSystem.Models
{
    public class LoginModel
    {
        public int id { get; set; }
        public string? username { get; set; }
        public string? password { get; set; }
    }
}
```

### Models/DashboardModel.cs
```csharp
namespace LMSystem.Models
{
    public class DashboardModel
    {
        public int TotalStudents { get; set; }
        public int TotalBooks { get; set; }
        public int TotalLibrarians { get; set; }
        public int TotalBorrowings { get; set; }
    }
}
```
> **Fix applied:** the source model was missing `TotalBorrowings` even though the Dashboard view references `@Model.TotalBorrowings` — that would have failed at build/runtime. Added here, and the controller below fills it in.

### Models/LibrarianModel.cs
```csharp
namespace LMSystem.Models
{
    public class LibrarianModel
    {
        public int LibrarianId { get; set; }
        public string? Name { get; set; }
        public int Age { get; set; }
        public string? Phone { get; set; }
    }
}
```

### Models/StudentModel.cs
```csharp
namespace LMSystem.Models
{
    public class StudentModel
    {
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}
```
> **Fix applied:** the source seed-data SQL included `Gender` and `Address` columns that don't exist anywhere in this model or its controller. Dropped from the schema below to match what the code actually uses.

---

## 5. ViewModels

### ViewModels/BorrowViewModel.cs
```csharp
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace LMSystem.ViewModels
{
    public class BorrowViewModel
    {
        [Required]
        public int BookId { get; set; }

        [BindNever]
        public string? BookTitle { get; set; }

        [Required(ErrorMessage = "Your name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string? BorrowerName { get; set; }

        [Required(ErrorMessage = "Your email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? BorrowerEmail { get; set; }

        [Required(ErrorMessage = "Your Phone Number is required.")]
        [Phone(ErrorMessage = "Invalid Phone Number")]
        public string? Phone { get; set; }
    }
}
```

### ViewModels/ReturnViewModel.cs
```csharp
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace LMSystem.ViewModels
{
    public class ReturnViewModel
    {
        [Required]
        public int BorrowRecordId { get; set; }

        [BindNever]
        public string? BookTitle { get; set; }

        [BindNever]
        public string? BorrowerName { get; set; }

        [BindNever]
        public DateTime? BorrowDate { get; set; }
    }
}
```

---

## 6. Program.cs

```csharp
using LMSystem.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<LibraryContext>(
    options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Books}/{action=Index}/{id?}");

app.Run();
```
> **Note on the default route:** the source material actually toggles this between `Books` and `Login` across sessions. This final version defaults to `Books` (matches the last documented state — the app is directly browsable without logging in first). If you want the Login page to be the true entry point, change `controller=Books` to `controller=Login`. See §11 for the security caveat either way.

---

## 7. Database Setup

**Step A — EF Core migration (Books + BorrowRecords tables, with seed data):**
```
Add-Migration InitialCreate
Update-Database
```
This creates the `LMS` database plus `Books` and `BorrowRecords` tables, seeded via `OnModelCreating`.

**Step B — Raw SQL for the remaining tables** (run against the `LMS` database created above):
```sql
USE LMS;
GO

-- Login table
CREATE TABLE logintab (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50),
    Password NVARCHAR(50)
);

INSERT INTO logintab (Username, Password) VALUES ('admin', '12345');
INSERT INTO logintab (Username, Password) VALUES ('mycodingproject', 'myc546');
INSERT INTO logintab (Username, Password) VALUES ('my', 'myc');

-- Librarians table
CREATE TABLE Librarians (
    LibrarianId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100),
    Age INT,
    Phone NVARCHAR(20)
);

INSERT INTO Librarians (Name, Age, Phone) VALUES
('Sarah Connor', 34, '555-0201'),
('John Doe', 28, '555-0202'),
('Michael Scott', 45, '555-0203'),
('Ellen Ripley', 39, '555-0204'),
('James Bond', 40, '555-0205');

-- Students table
CREATE TABLE Students (
    StudentId INT PRIMARY KEY IDENTITY(1,1),
    Student_Name NVARCHAR(100),
    Email NVARCHAR(100),
    Phone_Number NVARCHAR(20)
);

INSERT INTO Students (Student_Name, Email, Phone_Number) VALUES
('Alice Johnson', 'alice.j@email.com', '555-0101'),
('Bob Smith', 'bob.smith@email.com', '555-0102'),
('Charlie Brown', 'charlie.b@email.com', '555-0103'),
('Diana Prince', 'diana.p@email.com', '555-0104'),
('Evan Wright', 'evan.w@email.com', '555-0105');
```

**Entity-relationship summary:**
- `Books (1) ── (many) BorrowRecords` via `BookId` FK
- `Students`, `Librarians`, `logintab` are standalone tables (no FKs) — they're managed by hand-rolled ADO.NET, not EF, so there's no navigation between them and Books/BorrowRecords in code.

---

## 8. Controllers

### Controllers/BooksController.cs
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMSystem.Models;

namespace LMSystem.Controllers
{
    public class BooksController : Controller
    {
        private readonly LibraryContext _context;

        public BooksController(LibraryContext context)
        {
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            try
            {
                var books = await _context.Books
                    .Include(b => b.BorrowRecords)
                    .AsNoTracking()
                    .ToListAsync();
                return View(books);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the books.";
                return View("Error");
            }
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id == 0)
            {
                TempData["ErrorMessage"] = "Book ID was not provided.";
                return View("NotFound");
            }
            try
            {
                var book = await _context.Books.FirstOrDefaultAsync(m => m.BookId == id);
                if (book == null)
                {
                    TempData["ErrorMessage"] = $"No book found with ID {id}.";
                    return View("NotFound");
                }
                return View(book);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the book details.";
                return View("Error");
            }
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Books.Add(book);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Successfully added the book: {book.Title}.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "An error occurred while adding the book.";
                    return View(book);
                }
            }
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0)
            {
                TempData["ErrorMessage"] = "Book ID was not provided for editing.";
                return View("NotFound");
            }
            try
            {
                var book = await _context.Books.AsNoTracking().FirstOrDefaultAsync(m => m.BookId == id);
                if (book == null)
                {
                    TempData["ErrorMessage"] = $"No book found with ID {id} for editing.";
                    return View("NotFound");
                }
                return View(book);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the book for editing.";
                return View("Error");
            }
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, Book book)
        {
            if (id == null || id == 0)
            {
                TempData["ErrorMessage"] = "Book ID was not provided for updating.";
                return View("NotFound");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var existingBook = await _context.Books.FindAsync(id);
                    if (existingBook == null)
                    {
                        TempData["ErrorMessage"] = $"No book found with ID {id} for updating.";
                        return View("NotFound");
                    }
                    existingBook.Title = book.Title;
                    existingBook.Author = book.Author;
                    existingBook.ISBN = book.ISBN;
                    existingBook.PublishedDate = book.PublishedDate;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Successfully updated the book: {book.Title}.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.BookId))
                    {
                        TempData["ErrorMessage"] = $"No book found with ID {book.BookId} during concurrency check.";
                        return View("NotFound");
                    }
                    TempData["ErrorMessage"] = "A concurrency error occurred during the update.";
                    return View("Error");
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "An error occurred while updating the book.";
                    return View("Error");
                }
            }
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                TempData["ErrorMessage"] = "Book ID was not provided for deletion.";
                return View("NotFound");
            }
            try
            {
                var book = await _context.Books.AsNoTracking().FirstOrDefaultAsync(m => m.BookId == id);
                if (book == null)
                {
                    TempData["ErrorMessage"] = $"No book found with ID {id} for deletion.";
                    return View("NotFound");
                }
                return View(book);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the book for deletion.";
                return View("Error");
            }
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var book = await _context.Books.FindAsync(id);
                if (book == null)
                {
                    TempData["ErrorMessage"] = $"No book found with ID {id} for deletion.";
                    return View("NotFound");
                }
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Successfully deleted the book: {book.Title}.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the book.";
                return View("Error");
            }
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.BookId == id);
        }
    }
}
```

### Controllers/BorrowController.cs
```csharp
using LMSystem.Models;
using LMSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSystem.Controllers
{
    public class BorrowController : Controller
    {
        private readonly LibraryContext _context;

        public BorrowController(LibraryContext context)
        {
            _context = context;
        }

        // GET: Borrow/Create/5
        public async Task<IActionResult> Create(int? bookId)
        {
            if (bookId == null || bookId == 0)
            {
                TempData["ErrorMessage"] = "Book ID was not provided for borrowing.";
                return View("NotFound");
            }
            try
            {
                var book = await _context.Books.FindAsync(bookId);
                if (book == null)
                {
                    TempData["ErrorMessage"] = $"No book found with ID {bookId} to borrow.";
                    return View("NotFound");
                }
                if (!book.IsAvailable)
                {
                    TempData["ErrorMessage"] = $"The book '{book.Title}' is currently not available for borrowing.";
                    return View("NotAvailable");
                }
                var borrowViewModel = new BorrowViewModel
                {
                    BookId = book.BookId,
                    BookTitle = book.Title
                };
                return View(borrowViewModel);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the borrow form.";
                return View("Error");
            }
        }

        // POST: Borrow/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BorrowViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                var book = await _context.Books.FindAsync(model.BookId);
                if (book == null)
                {
                    TempData["ErrorMessage"] = $"No book found with ID {model.BookId} to borrow.";
                    return View("NotFound");
                }
                if (!book.IsAvailable)
                {
                    TempData["ErrorMessage"] = $"The book '{book.Title}' is already borrowed.";
                    return View("NotAvailable");
                }
                var borrowRecord = new BorrowRecord
                {
                    BookId = book.BookId,
                    BorrowerName = model.BorrowerName,
                    BorrowerEmail = model.BorrowerEmail,
                    Phone = model.Phone,
                    BorrowDate = DateTime.UtcNow
                };
                book.IsAvailable = false;
                _context.BorrowRecords.Add(borrowRecord);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Successfully borrowed the book: {book.Title}.";
                return RedirectToAction("Index", "Books");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while processing the borrowing action.";
                return View("Error");
            }
        }

        // GET: Borrow/Return/5
        public async Task<IActionResult> Return(int? borrowRecordId)
        {
            if (borrowRecordId == null || borrowRecordId == 0)
            {
                TempData["ErrorMessage"] = "Borrow Record ID was not provided for returning.";
                return View("NotFound");
            }
            try
            {
                var borrowRecord = await _context.BorrowRecords
                    .Include(br => br.Book)
                    .FirstOrDefaultAsync(br => br.BorrowRecordId == borrowRecordId);
                if (borrowRecord == null)
                {
                    TempData["ErrorMessage"] = $"No borrow record found with ID {borrowRecordId} to return.";
                    return View("NotFound");
                }
                if (borrowRecord.ReturnDate != null)
                {
                    TempData["ErrorMessage"] = $"The borrow record for '{borrowRecord.Book!.Title}' has already been returned.";
                    return View("AlreadyReturned");
                }
                var returnViewModel = new ReturnViewModel
                {
                    BorrowRecordId = borrowRecord.BorrowRecordId,
                    BookTitle = borrowRecord.Book!.Title,
                    BorrowerName = borrowRecord.BorrowerName,
                    BorrowDate = borrowRecord.BorrowDate
                };
                return View(returnViewModel);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the return confirmation.";
                return View("Error");
            }
        }

        // POST: Borrow/Return/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(ReturnViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                var borrowRecord = await _context.BorrowRecords
                    .Include(br => br.Book)
                    .FirstOrDefaultAsync(br => br.BorrowRecordId == model.BorrowRecordId);
                if (borrowRecord == null)
                {
                    TempData["ErrorMessage"] = $"No borrow record found with ID {model.BorrowRecordId} to return.";
                    return View("NotFound");
                }
                if (borrowRecord.ReturnDate != null)
                {
                    TempData["ErrorMessage"] = $"The borrow record for '{borrowRecord.Book!.Title}' has already been returned.";
                    return View("AlreadyReturned");
                }
                borrowRecord.ReturnDate = DateTime.UtcNow;
                borrowRecord.Book!.IsAvailable = true;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Successfully returned the book: {borrowRecord.Book!.Title}.";
                return RedirectToAction("Index", "Books");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while processing the return action.";
                return View("Error");
            }
        }
    }
}
```

### Controllers/LoginController.cs
```csharp
using LMSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace LMSystem.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public List<LoginModel> PutValue()
        {
            var users = new List<LoginModel>
            {
                new LoginModel { id = 1, username = "admin", password = "12345" },
                new LoginModel { id = 2, username = "mycodingproject", password = "myc546" },
                new LoginModel { id = 3, username = "my", password = "myc" },
            };
            return users;
        }

        [HttpPost]
        public IActionResult Verify(LoginModel usr)
        {
            var u = PutValue();
            var ue = u.Where(x => x.username!.Equals(usr.username));
            var up = ue.Where(x => x.password!.Equals(usr.password));

            if (up.Count() == 1)
            {
                TempData["message"] = "Login Success";
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                ViewBag.message = "Login Failed";
                return View("Index");
            }
        }
    }
}
```
> **Fix applied:** added the third seed user (`my` / `myc`, the "Librarian" account) to `PutValue()` — the source code's in-memory list only had `admin` and `mycodingproject`, so the Librarian login inserted into `logintab` had no matching in-code check.

### Controllers/DashboardController.cs
```csharp
using LMSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace LMSystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IConfiguration _config;

        public DashboardController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            var model = new DashboardModel();

            using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Students", connection))
                {
                    model.TotalStudents = (int)cmd.ExecuteScalar();
                }
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Books", connection))
                {
                    model.TotalBooks = (int)cmd.ExecuteScalar();
                }
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Librarians", connection))
                {
                    model.TotalLibrarians = (int)cmd.ExecuteScalar();
                }
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM BorrowRecords", connection))
                {
                    model.TotalBorrowings = (int)cmd.ExecuteScalar();
                }
            }

            return View(model);
        }
    }
}
```
> **Fix applied:** the source hardcoded the connection string directly in this controller (bypassing `appsettings.json`) and left the borrowings count commented out. Now uses `IConfiguration` (consistent with the Student/Librarian controllers) and actually populates `TotalBorrowings`.

### Controllers/StudentController.cs
```csharp
using LMSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace LMSystem.Controllers
{
    public class StudentController : Controller
    {
        private readonly IConfiguration _config;

        public StudentController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            var students = new List<StudentModel>();
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new SqlCommand("SELECT * FROM Students", con);
            con.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                students.Add(new StudentModel
                {
                    StudentId = (int)reader["StudentId"],
                    StudentName = reader["Student_Name"].ToString(),
                    Email = reader["Email"].ToString(),
                    Phone = reader["Phone_Number"].ToString()
                });
            }
            return View(students);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(StudentModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new SqlCommand("INSERT INTO Students (Student_Name, Email, Phone_Number) VALUES (@Name, @Email, @Phone)", con);
            cmd.Parameters.AddWithValue("@Name", model.StudentName);
            cmd.Parameters.AddWithValue("@Email", model.Email);
            cmd.Parameters.AddWithValue("@Phone", model.Phone);
            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            StudentModel student = new();
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new SqlCommand("SELECT * FROM Students WHERE StudentId=@id", con);
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                student.StudentId = (int)reader["StudentId"];
                student.StudentName = reader["Student_Name"].ToString();
                student.Email = reader["Email"].ToString();
                student.Phone = reader["Phone_Number"].ToString();
            }
            return View(student);
        }

        [HttpPost]
        public IActionResult Edit(StudentModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new SqlCommand("UPDATE Students SET Student_Name=@Name, Email=@Email, Phone_Number=@Phone WHERE StudentId=@id", con);
            cmd.Parameters.AddWithValue("@Name", model.StudentName);
            cmd.Parameters.AddWithValue("@Email", model.Email);
            cmd.Parameters.AddWithValue("@Phone", model.Phone);
            cmd.Parameters.AddWithValue("@id", model.StudentId);
            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new SqlCommand("DELETE FROM Students WHERE StudentId=@id", con);
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }
    }
}
```

### Controllers/LibrarianController.cs
```csharp
using LMSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace LMSystem.Controllers
{
    public class LibrarianController : Controller
    {
        private readonly IConfiguration _config;

        public LibrarianController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            var librarians = new List<LibrarianModel>();
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new SqlCommand("SELECT * FROM Librarians", con);
            con.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                librarians.Add(new LibrarianModel
                {
                    LibrarianId = (int)reader["LibrarianId"],
                    Name = reader["Name"].ToString(),
                    Age = (int)reader["Age"],
                    Phone = reader["Phone"].ToString()
                });
            }
            return View(librarians);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(LibrarianModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new SqlCommand("INSERT INTO Librarians (Name, Age, Phone) VALUES (@Name, @Age, @Phone)", con);
            cmd.Parameters.AddWithValue("@Name", model.Name);
            cmd.Parameters.AddWithValue("@Age", model.Age);
            cmd.Parameters.AddWithValue("@Phone", model.Phone);
            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            LibrarianModel librarian = new();
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new SqlCommand("SELECT * FROM Librarians WHERE LibrarianId=@id", con);
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                librarian.LibrarianId = (int)reader["LibrarianId"];
                librarian.Name = reader["Name"].ToString();
                librarian.Age = (int)reader["Age"];
                librarian.Phone = reader["Phone"].ToString();
            }
            return View(librarian);
        }

        [HttpPost]
        public IActionResult Edit(LibrarianModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new SqlCommand("UPDATE Librarians SET Name=@Name, Age=@Age, Phone=@Phone WHERE LibrarianId=@id", con);
            cmd.Parameters.AddWithValue("@Name", model.Name);
            cmd.Parameters.AddWithValue("@Age", model.Age);
            cmd.Parameters.AddWithValue("@Phone", model.Phone);
            cmd.Parameters.AddWithValue("@id", model.LibrarianId);
            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            using var con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new SqlCommand("DELETE FROM Librarians WHERE LibrarianId=@id", con);
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }
    }
}
```

---

## 9. Views

### Views/Shared/_Layout.cshtml
```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - LibraryManagement</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link rel="stylesheet" href="~/css/site.css" />
</head>
<body>
    <header>
        <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
            <div class="container-fluid">
                <a class="navbar-brand" href="/">LibraryManagement</a>
                <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
                    <span class="navbar-toggler-icon"></span>
                </button>
                <div class="collapse navbar-collapse" id="navbarNav">
                    <ul class="navbar-nav">
                        <li class="nav-item"><a class="nav-link" href="/Books/Index">Books</a></li>
                        <li class="nav-item"><a class="nav-link" href="/Student/Index">Students</a></li>
                        <li class="nav-item"><a class="nav-link" href="/Librarian/Index">Librarians</a></li>
                        <li class="nav-item"><a class="nav-link" href="/Login/Index">Login</a></li>
                        <li class="nav-item"><a class="nav-link" href="/Login">Logout</a></li>
                    </ul>
                </div>
            </div>
        </nav>
    </header>
    <main>
        <div class="container mt-4">
            @RenderBody()
        </div>
    </main>
    <footer class="footer mt-auto py-3 bg-dark text-light border-top border-secondary">
        <div class="container text-center">
            <span class="text-secondary">&copy; @DateTime.Now.Year - LibraryManagement. All rights reserved.</span>
        </div>
    </footer>
    <script src="https://cdn.jsdelivr.net/npm/jquery@3.7.0/dist/jquery.min.js"></script>
    @RenderSection("Scripts", required: false)
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>
```
> **Fix applied:** the source pointed jQuery at `~/lib/jquery/dist/jquery.min.js`, which only exists if LibMan/npm client-side libraries were separately restored during scaffolding. Switched to a CDN link so the layout works without that extra step.

### Views/Books/Index.cshtml
```html
@model IEnumerable<LMSystem.Models.Book>
@{
    ViewData["Title"] = "Books List";
}
@if (TempData["SuccessMessage"] != null)
{
    <div class="alert alert-success alert-dismissible fade show" role="alert">
        @TempData["SuccessMessage"]
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    </div>
}
<h2>@ViewData["Title"]</h2>
<table class="table table-striped table-hover">
    <thead class="table-dark">
        <tr>
            <th>Title</th><th>Author</th><th>ISBN</th><th>Published Date</th><th>Availability</th><th>Actions</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var book in Model)
        {
            <tr id="bookRow-@book.BookId">
                <td>@book.Title</td>
                <td>@book.Author</td>
                <td>@book.ISBN</td>
                <td>@book.PublishedDate.ToString("yyyy-MM-dd")</td>
                <td>
                    @if (book.IsAvailable)
                    {
                        <span class="badge bg-success">Available</span>
                    }
                    else
                    {
                        <span class="badge bg-danger">Borrowed</span>
                    }
                </td>
                <td>
                    <a asp-action="Details" asp-route-id="@book.BookId" class="btn btn-info btn-sm">Details</a>
                    <a asp-action="Edit" asp-route-id="@book.BookId" class="btn btn-warning btn-sm">Edit</a>
                    <a asp-action="Delete" asp-route-id="@book.BookId" class="btn btn-danger btn-sm">Delete</a>
                    @if (book.IsAvailable)
                    {
                        <a asp-controller="Borrow" asp-action="Create" asp-route-bookId="@book.BookId" class="btn btn-primary btn-sm">Borrow</a>
                    }
                    else
                    {
                        var activeBorrowRecord = book.BorrowRecords?.FirstOrDefault(br => br.ReturnDate == null);
                        if (activeBorrowRecord != null)
                        {
                            <a asp-controller="Borrow" asp-action="Return" asp-route-borrowRecordId="@activeBorrowRecord.BorrowRecordId" class="btn btn-success btn-sm">Return</a>
                        }
                        else
                        {
                            <span class="text-muted">No active borrow record</span>
                        }
                    }
                </td>
            </tr>
        }
    </tbody>
</table>
<a asp-action="Create" class="btn btn-primary">Add New Book</a>
```

### Views/Books/Details.cshtml
```html
@model LMSystem.Models.Book
@{
    ViewData["Title"] = "Book Details";
}
<div class="container mt-5">
    <div class="row">
        <div class="col-md-9">
            <div class="card h-100">
                <div class="card-header bg-primary text-white">
                    <h3 class="card-title">@Model.Title</h3>
                </div>
                <div class="card-body">
                    <dl class="row">
                        <dt class="col-sm-4">Author:</dt><dd class="col-sm-8">@Model.Author</dd>
                        <dt class="col-sm-4">ISBN:</dt><dd class="col-sm-8">@Model.ISBN</dd>
                        <dt class="col-sm-4">Published Date:</dt><dd class="col-sm-8">@Model.PublishedDate.ToString("yyyy-MM-dd")</dd>
                        <dt class="col-sm-4">Availability:</dt>
                        <dd class="col-sm-8">
                            @if (Model.IsAvailable)
                            {
                                <span class="badge bg-success">Available</span>
                            }
                            else
                            {
                                <span class="badge bg-danger">Checked Out</span>
                            }
                        </dd>
                    </dl>
                </div>
                <div class="card-footer">
                    <a asp-action="Edit" asp-route-id="@Model.BookId" class="btn btn-warning me-2">Edit</a>
                    <a asp-action="Index" class="btn btn-secondary">Back to List</a>
                </div>
            </div>
        </div>
    </div>
</div>
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.5/font/bootstrap-icons.css">
```
> Simplified from the source: removed the hardcoded `openlibrary.org` cover-image URL, since it only ever resolves for one specific ISBN and shows a broken image for every other book.

### Views/Books/Create.cshtml
```html
@model LMSystem.Models.Book
@{
    ViewData["Title"] = "Add New Book";
}
<div class="container mt-5">
    <div class="row justify-content-center">
        <div class="col-md-8">
            <div class="card shadow-sm">
                <div class="card-header bg-primary text-white"><h4 class="mb-0">Add New Book</h4></div>
                <div class="card-body">
                    <form asp-action="Create" asp-controller="Books" method="post">
                        <div asp-validation-summary="All" class="alert alert-danger d-none"></div>
                        <div class="mb-3">
                            <label asp-for="Title" class="form-label"></label>
                            <input asp-for="Title" class="form-control" placeholder="Enter book title" autofocus />
                            <span asp-validation-for="Title" class="text-danger"></span>
                        </div>
                        <div class="mb-3">
                            <label asp-for="Author" class="form-label"></label>
                            <input asp-for="Author" class="form-control" placeholder="Enter author's name" />
                            <span asp-validation-for="Author" class="text-danger"></span>
                        </div>
                        <div class="mb-3">
                            <label asp-for="ISBN" class="form-label"></label>
                            <input asp-for="ISBN" class="form-control" placeholder="e.g., 978-1234567890" />
                            <span asp-validation-for="ISBN" class="text-danger"></span>
                        </div>
                        <div class="mb-4">
                            <label asp-for="PublishedDate" class="form-label"></label>
                            <input asp-for="PublishedDate" class="form-control" type="date" />
                            <span asp-validation-for="PublishedDate" class="text-danger"></span>
                        </div>
                        <div class="d-flex justify-content-end">
                            <button type="submit" class="btn btn-primary me-2">Add Book</button>
                            <a asp-action="Index" class="btn btn-secondary">Cancel</a>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    </div>
</div>
@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

### Views/Books/Edit.cshtml
```html
@model LMSystem.Models.Book
@{
    ViewData["Title"] = "Edit Book";
}
<div class="container mt-5">
    <div class="row justify-content-center">
        <div class="col-md-8">
            <div class="card shadow-sm">
                <div class="card-header bg-primary text-white"><h4 class="mb-0">Edit Book</h4></div>
                <div class="card-body">
                    <form asp-action="Edit" asp-controller="Books" method="post">
                        <div asp-validation-summary="All" class="alert alert-danger d-none"></div>
                        <input type="hidden" asp-for="BookId" />
                        <div class="mb-3">
                            <label asp-for="Title" class="form-label"></label>
                            <input asp-for="Title" class="form-control" />
                            <span asp-validation-for="Title" class="text-danger"></span>
                        </div>
                        <div class="mb-3">
                            <label asp-for="Author" class="form-label"></label>
                            <input asp-for="Author" class="form-control" />
                            <span asp-validation-for="Author" class="text-danger"></span>
                        </div>
                        <div class="mb-3">
                            <label asp-for="ISBN" class="form-label"></label>
                            <input asp-for="ISBN" class="form-control" />
                            <span asp-validation-for="ISBN" class="text-danger"></span>
                        </div>
                        <div class="mb-4">
                            <label asp-for="PublishedDate" class="form-label"></label>
                            <input asp-for="PublishedDate" class="form-control" type="date" />
                            <span asp-validation-for="PublishedDate" class="text-danger"></span>
                        </div>
                        <div class="d-flex justify-content-end">
                            <button type="submit" class="btn btn-primary me-2">Save Changes</button>
                            <a asp-action="Index" class="btn btn-secondary">Cancel</a>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    </div>
</div>
@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

### Views/Books/Delete.cshtml
```html
@model LMSystem.Models.Book
@{
    ViewData["Title"] = "Delete Book";
}
<div class="container mt-5">
    <div class="row justify-content-center">
        <div class="col-md-8">
            <div class="card border-danger">
                <div class="card-header bg-danger text-white"><h4 class="mb-0">Confirm Delete</h4></div>
                <div class="card-body">
                    <div class="alert alert-danger" role="alert">
                        <h4 class="alert-heading">Are you sure you want to delete this book?</h4>
                        <p>Once deleted, this action cannot be undone.</p>
                    </div>
                    <dl class="row">
                        <dt class="col-sm-4">Title:</dt><dd class="col-sm-8">@Model.Title</dd>
                        <dt class="col-sm-4">Author:</dt><dd class="col-sm-8">@Model.Author</dd>
                        <dt class="col-sm-4">ISBN:</dt><dd class="col-sm-8">@Model.ISBN</dd>
                        <dt class="col-sm-4">Published Date:</dt><dd class="col-sm-8">@Model.PublishedDate.ToString("yyyy-MM-dd")</dd>
                        <dt class="col-sm-4">Availability:</dt>
                        <dd class="col-sm-8">
                            @if (Model.IsAvailable) { <span class="badge bg-success">Available</span> }
                            else { <span class="badge bg-danger">Checked Out</span> }
                        </dd>
                    </dl>
                </div>
                <div class="card-footer d-flex justify-content-end">
                    <form asp-action="Delete" asp-controller="Books" method="post" class="me-2">
                        <input type="hidden" asp-for="BookId" />
                        <button type="submit" class="btn btn-danger">Delete</button>
                    </form>
                    <a asp-action="Index" asp-controller="Books" class="btn btn-secondary">Cancel</a>
                </div>
            </div>
        </div>
    </div>
</div>
```

### Views/Shared/NotFound.cshtml
```html
@{
    ViewData["Title"] = "Page Not Found";
    var errorMessage = TempData["ErrorMessage"] as string;
}
<div class="container text-center mt-5">
    <h1 class="display-4 text-danger">Resource Not Found</h1>
    <p class="lead">
        @if (!string.IsNullOrEmpty(errorMessage)) { @errorMessage }
        else { <text>The page you are looking for does not exist.</text> }
    </p>
    <a asp-action="Index" asp-controller="Books" class="btn btn-primary">Back to Book List</a>
</div>
```
> **Fix applied:** the source read `ViewBag.ErrorMessage`, but every controller actually sets `TempData["ErrorMessage"]` — those are two different bags in ASP.NET Core MVC. Switched the view to read from `TempData` so the message actually shows up.

### Views/Shared/NotAvailable.cshtml
```html
@{
    ViewData["Title"] = "Book Not Available";
    var errorMessage = TempData["ErrorMessage"] as string;
}
<div class="container text-center mt-5">
    <h1 class="display-4 text-warning">Book Not Available</h1>
    <p class="lead">
        @if (!string.IsNullOrEmpty(errorMessage)) { @errorMessage }
        else { <text>The book you are trying to borrow is currently not available.</text> }
    </p>
    <a asp-action="Index" asp-controller="Books" class="btn btn-primary">Back to Book List</a>
</div>
```
> Same `TempData` fix applied here.

### Views/Shared/AlreadyReturned.cshtml
```html
@{
    ViewData["Title"] = "Book Already Returned";
    var errorMessage = TempData["ErrorMessage"] as string;
}
<div class="container text-center mt-5">
    <h1 class="display-4 text-info">Book Already Returned</h1>
    <p class="lead">
        @if (!string.IsNullOrEmpty(errorMessage)) { @errorMessage }
        else { <text>The borrow record for this book has already been returned.</text> }
    </p>
    <a asp-action="Index" asp-controller="Books" class="btn btn-primary">Back to Book List</a>
</div>
```
> Same `TempData` fix applied here.

### Views/Borrow/Create.cshtml
```html
@model LMSystem.ViewModels.BorrowViewModel
@{
    ViewData["Title"] = "Borrow Book";
}
<div class="container mt-5">
    <h2>Borrow Book</h2>
    <div class="card">
        <div class="card-header"><strong>@Model.BookTitle</strong></div>
        <div class="card-body">
            <form asp-action="Create" method="post">
                <input type="hidden" asp-for="BookId" />
                <div class="mb-3">
                    <label asp-for="BorrowerName" class="form-label"></label>
                    <input asp-for="BorrowerName" class="form-control" />
                    <span asp-validation-for="BorrowerName" class="text-danger"></span>
                </div>
                <div class="mb-3">
                    <label asp-for="BorrowerEmail" class="form-label"></label>
                    <input asp-for="BorrowerEmail" class="form-control" />
                    <span asp-validation-for="BorrowerEmail" class="text-danger"></span>
                </div>
                <div class="mb-3">
                    <label asp-for="Phone" class="form-label"></label>
                    <input asp-for="Phone" class="form-control" />
                    <span asp-validation-for="Phone" class="text-danger"></span>
                </div>
                <button type="submit" class="btn btn-primary">Confirm Borrow</button>
                <a asp-action="Index" asp-controller="Books" class="btn btn-secondary">Cancel</a>
            </form>
        </div>
    </div>
</div>
@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

### Views/Borrow/Return.cshtml
```html
@model LMSystem.ViewModels.ReturnViewModel
@{
    ViewData["Title"] = "Return Book";
}
<div class="container mt-5">
    <h2>Return Book</h2>
    <div class="card">
        <div class="card-header"><strong>@Model.BookTitle</strong></div>
        <div class="card-body">
            <p><strong>Borrower Name:</strong> @Model.BorrowerName</p>
            <p><strong>Borrow Date:</strong> @Model.BorrowDate?.ToString("yyyy-MM-dd HH:mm:ss")</p>
            <form asp-action="Return" method="post">
                <input type="hidden" asp-for="BorrowRecordId" />
                <button type="submit" class="btn btn-success">Confirm Return</button>
                <a asp-action="Index" asp-controller="Books" class="btn btn-secondary">Cancel</a>
            </form>
        </div>
    </div>
</div>
```

### Views/Login/Index.cshtml
```html
@model LMSystem.Models.LoginModel
@{
    ViewData["Title"] = "Login";
}
<div class="container mt-5">
    <div class="row justify-content-center">
        <div class="col-md-4">
            <div class="card shadow">
                <div class="card-header bg-dark text-white text-center"><h4>Library Management Login</h4></div>
                <div class="card-body">
                    @if (ViewBag.message != null)
                    {
                        <div class="alert alert-danger">@ViewBag.message</div>
                    }
                    <form asp-action="Verify" method="post" asp-controller="Login">
                        <div asp-validation-summary="ModelOnly" class="text-danger mb-3"></div>
                        <div class="form-group mb-3">
                            <label asp-for="username" class="form-label">Username / Email</label>
                            <input asp-for="username" class="form-control" placeholder="Enter username" />
                            <span asp-validation-for="username" class="text-danger"></span>
                        </div>
                        <div class="form-group mb-3">
                            <label asp-for="password" class="form-label">Password</label>
                            <input asp-for="password" class="form-control" type="password" placeholder="Enter password" />
                            <span asp-validation-for="password" class="text-danger"></span>
                        </div>
                        <div class="form-group mb-3">
                            <label for="remember-me" class="text-info">
                                <span>Remember me</span>
                                <input id="remember-me" name="remember-me" type="checkbox">
                            </label><br>
                            <button type="submit" class="btn btn-info btn-md">Login</button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    </div>
</div>
```
> **Fix applied:** added `type="password"` to the password input (source left it as plain text, meaning the password was visible on screen while typing) and rendered the "Login Failed" message that `ViewBag.message` carries — the source never actually displayed it in the view.

### Views/Dashboard/Index.cshtml
```html
@model LMSystem.Models.DashboardModel
@{
    ViewData["Title"] = "Dashboard";
}
@if (TempData["message"] != null)
{
    <div class="alert alert-success alert-dismissible fade show" role="alert">
        @TempData["message"]
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    </div>
}
<div class="container mt-5">
    <div class="row mb-4">
        <div class="col-12 text-center text-md-start">
            <h2 class="fw-bold border-bottom pb-2 text-dark">Admin Dashboard</h2>
        </div>
    </div>
    <div class="row g-4">
        <div class="col-12 col-sm-6 col-lg-3">
            <div class="card h-100 border-0 shadow-sm text-white" style="background-color: #4e73df;">
                <div class="card-body d-flex flex-column justify-content-between p-4">
                    <div>
                        <h6 class="text-uppercase fw-semibold opacity-75 mb-1">Total Students</h6>
                        <h2 class="display-6 fw-bold mb-0">@Model.TotalStudents</h2>
                    </div>
                </div>
            </div>
        </div>
        <div class="col-12 col-sm-6 col-lg-3">
            <div class="card h-100 border-0 shadow-sm text-white" style="background-color: #1cc88a;">
                <div class="card-body d-flex flex-column justify-content-between p-4">
                    <div>
                        <h6 class="text-uppercase fw-semibold opacity-75 mb-1">Total Books</h6>
                        <h2 class="display-6 fw-bold mb-0">@Model.TotalBooks</h2>
                    </div>
                </div>
            </div>
        </div>
        <div class="col-12 col-sm-6 col-lg-3">
            <div class="card h-100 border-0 shadow-sm text-white" style="background-color: #36b9cc;">
                <div class="card-body d-flex flex-column justify-content-between p-4">
                    <div>
                        <h6 class="text-uppercase fw-semibold opacity-75 mb-1">Total Librarians</h6>
                        <h2 class="display-6 fw-bold mb-0">@Model.TotalLibrarians</h2>
                    </div>
                </div>
            </div>
        </div>
        <div class="col-12 col-sm-6 col-lg-3">
            <div class="card h-100 border-0 shadow-sm text-white" style="background-color: #f6c23e;">
                <div class="card-body d-flex flex-column justify-content-between p-4">
                    <div>
                        <h6 class="text-uppercase fw-semibold opacity-75 mb-1">Total Borrowings</h6>
                        <h2 class="display-6 fw-bold mb-0">@Model.TotalBorrowings</h2>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
```

### Views/Student/Index.cshtml
```html
@model List<LMSystem.Models.StudentModel>
@{
    ViewData["Title"] = "Students";
}
<a class="btn btn-primary mt-3 ms-2" href="/Student/Create">Add New Student</a>
<table class="table table-bordered mt-3">
    <thead>
        <tr><th>StudentID</th><th>StudentName</th><th>Email</th><th>Phone</th><th></th></tr>
    </thead>
    <tbody>
        @foreach (var item in Model)
        {
            <tr>
                <td>@item.StudentId</td>
                <td>@item.StudentName</td>
                <td>@item.Email</td>
                <td>@item.Phone</td>
                <td>
                    <a class="btn btn-warning btn-sm" href="/Student/Edit/@item.StudentId">Edit</a>
                    <a class="btn btn-danger btn-sm" href="/Student/Delete/@item.StudentId" onclick="return confirm('Are you sure?')">Delete</a>
                </td>
            </tr>
        }
    </tbody>
</table>
```

### Views/Student/Create.cshtml
```html
@model LMSystem.Models.StudentModel
@{
    ViewData["Title"] = "Add Student";
}
<h2>Add Student</h2>
<form asp-action="Create" method="post" onsubmit="return validateForm();">
    <div class="mb-3"><label>StudentName</label><input asp-for="StudentName" class="form-control" /></div>
    <div class="mb-3"><label>Email</label><input asp-for="Email" class="form-control" /></div>
    <div class="mb-3"><label>Phone</label><input asp-for="Phone" class="form-control" /></div>
    <button type="submit" class="btn btn-success">Add</button>
    <a asp-action="Index" class="btn btn-secondary">Cancel</a>
</form>
<script>
    function validateForm() {
        const studentname = document.querySelector('[name="StudentName"]').value.trim();
        const email = document.querySelector('[name="Email"]').value.trim();
        const phone = document.querySelector('[name="Phone"]').value.trim();
        if (!studentname || !email || !phone) {
            alert("All fields are required: StudentName, Email, and Phone.");
            return false;
        }
        return true;
    }
</script>
```

### Views/Student/Edit.cshtml
```html
@model LMSystem.Models.StudentModel
@{
    ViewData["Title"] = "Edit Student";
}
<h2>Edit Student</h2>
<form asp-action="Edit" method="post">
    <input type="hidden" asp-for="StudentId" />
    <div class="mb-3"><label>StudentName</label><input asp-for="StudentName" class="form-control" /></div>
    <div class="mb-3"><label>Email</label><input asp-for="Email" class="form-control" /></div>
    <div class="mb-3"><label>Phone</label><input asp-for="Phone" class="form-control" /></div>
    <button type="submit" class="btn btn-primary">Update</button>
    <a asp-action="Index" class="btn btn-secondary">Cancel</a>
</form>
```

### Views/Librarian/Index.cshtml
```html
@model List<LMSystem.Models.LibrarianModel>
@{
    ViewData["Title"] = "Librarians";
}
<a class="btn btn-primary mt-3 ms-2" href="/Librarian/Create">Add New Librarian</a>
<table class="table table-bordered mt-3">
    <thead>
        <tr><th>LibrarianID</th><th>Name</th><th>Age</th><th>Phone</th><th></th></tr>
    </thead>
    <tbody>
        @foreach (var item in Model)
        {
            <tr>
                <td>@item.LibrarianId</td>
                <td>@item.Name</td>
                <td>@item.Age</td>
                <td>@item.Phone</td>
                <td>
                    <a class="btn btn-warning btn-sm" href="/Librarian/Edit/@item.LibrarianId">Edit</a>
                    <a class="btn btn-danger btn-sm" href="/Librarian/Delete/@item.LibrarianId" onclick="return confirm('Are you sure?')">Delete</a>
                </td>
            </tr>
        }
    </tbody>
</table>
```

### Views/Librarian/Create.cshtml
```html
@model LMSystem.Models.LibrarianModel
@{
    ViewData["Title"] = "Add Librarian";
}
<h2>Add Librarian</h2>
<form asp-action="Create" method="post" onsubmit="return validateForm();">
    <div class="mb-3"><label>Name</label><input asp-for="Name" class="form-control" /></div>
    <div class="mb-3"><label>Age</label><input asp-for="Age" class="form-control" /></div>
    <div class="mb-3"><label>Phone</label><input asp-for="Phone" class="form-control" /></div>
    <button type="submit" class="btn btn-success">Add</button>
    <a asp-action="Index" class="btn btn-secondary">Cancel</a>
</form>
<script>
    function validateForm() {
        const name = document.querySelector('[name="Name"]').value.trim();
        const age = document.querySelector('[name="Age"]').value.trim();
        const phone = document.querySelector('[name="Phone"]').value.trim();
        if (!name || !age || !phone) {
            alert("All fields are required: Name, Age, Phone.");
            return false;
        }
        return true;
    }
</script>
```

### Views/Librarian/Edit.cshtml
```html
@model LMSystem.Models.LibrarianModel
@{
    ViewData["Title"] = "Edit Library";
}
<h2>Edit Library</h2>
<form asp-action="Edit" method="post">
    <input type="hidden" asp-for="LibrarianId" />
    <div class="mb-3"><label>Name</label><input asp-for="Name" class="form-control" /></div>
    <div class="mb-3"><label>Age</label><input asp-for="Age" class="form-control" /></div>
    <div class="mb-3"><label>Phone</label><input asp-for="Phone" class="form-control" /></div>
    <button type="submit" class="btn btn-primary">Update</button>
    <a asp-action="Index" class="btn btn-secondary">Cancel</a>
</form>
```

---

## 10. Manual Test Checklist

1. `Add-Migration InitialCreate` + `Update-Database` succeed; `Books` and `BorrowRecords` tables exist with 4 seeded books.
2. Run §7 Step B SQL; `logintab`, `Librarians`, `Students` tables exist with seed rows.
3. App launches to Books list (4 books, all "Available").
4. Books: Create, Details, Edit, Delete each work; validation blocks empty Title/Author/ISBN; ISBN regex rejects a malformed value (e.g. `12345`).
5. Borrow a book → status flips to "Borrowed", a "Return" button appears in its place.
6. Try to borrow that same book again by hitting `/Borrow/Create?bookId=<id>` directly → **Not Available** page shown.
7. Return the borrowed book → status flips back to "Available".
8. Try to return the same `BorrowRecordId` a second time (revisit the same `/Borrow/Return/<id>` URL) → **Already Returned** page shown.
9. Hit `/Books/Details/999` (nonexistent ID) → **Not Found** page shown.
10. `/Login` → log in with `admin` / `12345` → redirects to Dashboard, shows correct counts (4 students, 4 books, 5 librarians, borrowings = however many are currently active).
11. `/Login` → log in with `mycodingproject` / `myc546` and with `my` / `myc` → both succeed too.
12. `/Login` → wrong credentials → "Login Failed" message shown on the same page.
13. Students: Index/Create/Edit/Delete all work against the `Students` table.
14. Librarians: Index/Create/Edit/Delete all work against the `Librarians` table.

---

## 11. Known Limitations (carried over from the source material, not fixed here)

- **Login isn't actually enforced.** There's no `[Authorize]`, no session, no cookie — visiting `/Books`, `/Student`, `/Librarian`, or `/Dashboard` directly works whether or not you've logged in. "Logout" just links back to the Login page; it doesn't clear anything. If you want real access control, add `builder.Services.AddSession()` + `app.UseSession()`, store a flag in `Session` on successful `Verify()`, and check it in a base controller or action filter — this wasn't part of the original scope, so it's not built into the code above.
- **Passwords are stored and compared in plain text** (`logintab.Password`, `LoginController.Verify`). Fine for a course project; not something to carry into anything real.
- **Students/Librarians have no server-side validation** — the "required fields" check is JavaScript-only (`validateForm()`), so it can be bypassed by disabling JS or posting directly. Books/Borrow, by contrast, use proper Data Annotations + `ModelState.IsValid`.
- **Delete Book/Delete Student/Delete Librarian don't check for related BorrowRecords** or confirm cascading behavior — deleting a book with active borrow history isn't guarded against.

---

## 12. Explicitly Out of Scope (Future Sessions)

Per the source material, the next planned session covers:
- Searching and pagination for the **Books** module
- Searching and pagination for the **Librarian** module
- Searching and pagination for the **Students** module

Sessions 4 and 5 have not been provided yet — once available, this spec should be extended rather than restarted.

---

## 13. Test Credentials Summary

| Username | Password | Represents |
|---|---|---|
| admin | 12345 | Admin |
| mycodingproject | myc546 | Student |
| my | myc | Librarian |
