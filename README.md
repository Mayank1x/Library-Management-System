# Library Management System

A full-featured Library Management System built with ASP.NET Core MVC 8. This application manages books, students, librarians, and borrowing records, combining both Entity Framework Core and ADO.NET for data access.

## Features

- **Books Management**: Add, edit, view, and delete books in the library.
- **Borrow & Return System**: Checkout books to students and process returns, with automatic availability tracking.
- **Librarian Management**: Maintain records of library staff.
- **Student Management**: Keep track of registered students and their contact information.
- **Dashboard**: A quick overview of total books, students, librarians, and borrowing activity.
- **Authentication**: A secure login system for library administrators.

## Tech Stack

- **Framework**: .NET 8 / ASP.NET Core MVC
- **Database**: SQL Server (LocalDB)
- **ORM / Data Access**: 
  - Entity Framework Core 8.0.0 (Code-First) for Books and Borrow Records.
  - ADO.NET (`Microsoft.Data.SqlClient`) for Students, Librarians, Login, and Dashboard modules.
- **Frontend**: HTML5, Bootstrap 5.3.0, Bootstrap Icons 1.10.5
- **Validation**: Data Annotations

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) (installed with Visual Studio)
- IDE: Visual Studio 2022, Rider, or VS Code

## Getting Started

1. **Clone the repository**:
   ```bash
   git clone https://github.com/Mayank1x/Library-Management-System.git
   cd "Library Management System/LMSystem"
   ```

2. **Database Setup**:
   The application uses a SQL Server LocalDB named `LMS`. 
   
   First, run the Entity Framework Core migrations to create the database and the `Books` / `BorrowRecords` tables (this will also insert seed data for Books):
   ```bash
   dotnet ef database update
   ```
   
   Next, you need to create the remaining tables (`logintab`, `Librarians`, `Students`) using the provided raw SQL. You can execute the `seed.sql` or the scripts in the project against the `LMS` database.

3. **Run the Application**:
   ```bash
   dotnet run
   ```
   The application will start, and you can navigate to the local URL (usually `https://localhost:7000` or similar depending on `launchSettings.json`).

## Project Structure

- `Models/`: Data models and Entity Framework DbContext.
- `ViewModels/`: Models specifically tailored for the Views (e.g., Borrowing and Returning books).
- `Controllers/`: Handling web requests, business logic, and routing.
- `Views/`: Razor pages and UI layout using Bootstrap.
- `appsettings.json`: Configuration, including the database connection string.

## License

This project is licensed under the MIT License.
