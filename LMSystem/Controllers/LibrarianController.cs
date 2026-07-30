using Microsoft.AspNetCore.Authorization;
using LMSystem.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace LMSystem.Controllers
{
    [Authorize]
    public class LibrarianController : Controller
    {
        private readonly IConfiguration _config;

        public LibrarianController(IConfiguration config)
        {
            _config = config;
        }

        // GET: Librarian — now supports search + pagination
        public IActionResult Index(string? searchTerm, int page = 1)
        {
            if (page < 1) page = 1;
            int pageSize = 5;
            int offset = (page - 1) * pageSize;

            var librarians = new List<LibrarianModel>();
            int totalRecords = 0;

            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            string countQuery = "SELECT COUNT(*) FROM Librarians WHERE (@SearchTerm IS NULL OR Name LIKE CONCAT('%', @SearchTerm, '%'))";
            using (var countCmd = new MySqlCommand(countQuery, con))
            {
                countCmd.Parameters.AddWithValue("@SearchTerm", (object?)searchTerm ?? DBNull.Value);
                totalRecords = Convert.ToInt32(countCmd.ExecuteScalar());
            }

            string dataQuery = @"SELECT * FROM Librarians
                                  WHERE (@SearchTerm IS NULL OR Name LIKE CONCAT('%', @SearchTerm, '%'))
                                  ORDER BY LibrarianId
                                  LIMIT @PageSize OFFSET @Offset";
            using (var cmd = new MySqlCommand(dataQuery, con))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", (object?)searchTerm ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Offset", offset);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    librarians.Add(new LibrarianModel
                    {
                        LibrarianId = Convert.ToInt32(reader["LibrarianId"]),
                        Name = reader["Name"].ToString(),
                        Age = Convert.ToInt32(reader["Age"]),
                        Phone = reader["Phone"].ToString()
                    });
                }
            }

            var viewModel = new LibrarianIndexViewModel
            {
                Librarians = librarians,
                SearchTerm = searchTerm,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
            };

            return View(viewModel);
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

            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand("INSERT INTO Librarians (Name, Age, Phone) VALUES (@Name, @Age, @Phone)", con);
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
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand("SELECT * FROM Librarians WHERE LibrarianId=@id", con);
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

            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand("UPDATE Librarians SET Name=@Name, Age=@Age, Phone=@Phone WHERE LibrarianId=@id", con);
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
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand("DELETE FROM Librarians WHERE LibrarianId=@id", con);
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }
    }
}

