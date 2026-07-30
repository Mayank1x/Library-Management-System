using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LMSystem.Migrations
{
    /// <inheritdoc />
    public partial class ExpandCatalogAndAddPublications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    BookId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Author = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ISBN = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PublishedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsAvailable = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.BookId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Publications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Publisher = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PublishedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsAvailable = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publications", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BorrowRecords",
                columns: table => new
                {
                    BorrowRecordId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    BorrowerName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BorrowerEmail = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BorrowDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowRecords", x => x.BorrowRecordId);
                    table.ForeignKey(
                        name: "FK_BorrowRecords_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "BookId", "Author", "ISBN", "IsAvailable", "PublishedDate", "Title" },
                values: new object[,]
                {
                    { 1, "Andrew Hunt and David Thomas", "978-0201616224", true, new DateTime(2021, 10, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "The Pragmatic Programmer" },
                    { 2, "Robert C. Martin", "978-0132350884", true, new DateTime(2023, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Design Pattern using C#" },
                    { 3, "Pranaya Kumar Rout", "978-0451616235", true, new DateTime(2022, 11, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mastering ASP.NET Core" },
                    { 4, "Rakesh Kumar", "978-4562350123", true, new DateTime(2020, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "SQL Server with DBA" },
                    { 5, "Robert C. Martin", "978-0134494166", true, new DateTime(2017, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Clean Architecture" },
                    { 6, "Martin Fowler", "978-0134757599", true, new DateTime(2018, 11, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Refactoring" },
                    { 7, "Robert C. Martin", "978-0137081073", true, new DateTime(2011, 5, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "The Clean Coder" },
                    { 8, "Eric Evans", "978-0321125217", true, new DateTime(2003, 8, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Domain-Driven Design" },
                    { 9, "Kyle Simpson", "978-1491904244", true, new DateTime(2015, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "You Don't Know JS" },
                    { 10, "Thomas H. Cormen", "978-0262033848", true, new DateTime(2009, 7, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Introduction to Algorithms" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BorrowRecords_BookId",
                table: "BorrowRecords",
                column: "BookId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BorrowRecords");

            migrationBuilder.DropTable(
                name: "Publications");

            migrationBuilder.DropTable(
                name: "Books");
        }
    }
}
