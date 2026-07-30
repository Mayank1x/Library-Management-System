using Microsoft.AspNetCore.Authorization;
using LMSystem.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace LMSystem.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly IConfiguration _config;

        public StudentController(IConfiguration config)
        {
            _config = config;
        }

        // GET: Student — now supports search + pagination
        public IActionResult Index(string? searchTerm, int page = 1)
        {
            var viewModel = new StudentIndexViewModel
            {
                SearchTerm = searchTerm,
                CurrentPage = page < 1 ? 1 : page
            };

            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            con.Open();

            string searchCondition = "";
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchCondition = " WHERE Student_Name LIKE @Search OR Email LIKE @Search OR Phone_Number LIKE @Search";
            }

            string countQuery = $"SELECT COUNT(*) FROM Students{searchCondition}";
            using (var countCmd = new MySqlCommand(countQuery, con))
            {
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    countCmd.Parameters.AddWithValue("@Search", $"%{searchTerm}%");
                }
                int totalRecords = Convert.ToInt32(countCmd.ExecuteScalar());
                viewModel.TotalPages = (int)Math.Ceiling((double)totalRecords / viewModel.PageSize);
            }

            if (viewModel.CurrentPage > viewModel.TotalPages && viewModel.TotalPages > 0)
            {
                viewModel.CurrentPage = viewModel.TotalPages;
            }

            int offset = (viewModel.CurrentPage - 1) * viewModel.PageSize;
            string dataQuery = $@"SELECT * FROM Students{searchCondition}
                                   ORDER BY StudentId
                                   LIMIT @PageSize OFFSET @Offset";

            using (var dataCmd = new MySqlCommand(dataQuery, con))
            {
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    dataCmd.Parameters.AddWithValue("@Search", $"%{searchTerm}%");
                }
                dataCmd.Parameters.AddWithValue("@Offset", offset);
                dataCmd.Parameters.AddWithValue("@PageSize", viewModel.PageSize);

                using var reader = dataCmd.ExecuteReader();
                while (reader.Read())
                {
                    viewModel.Students.Add(new StudentModel
                    {
                        StudentId = Convert.ToInt32(reader["StudentId"]),
                        StudentName = reader["Student_Name"].ToString(),
                        Email = reader["Email"].ToString(),
                        Phone = reader["Phone_Number"].ToString()
                    });
                }
            }

            return View(viewModel);
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

            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand("INSERT INTO Students (Student_Name, Email, Phone_Number) VALUES (@Name, @Email, @Phone)", con);
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
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand("SELECT * FROM Students WHERE StudentId=@id", con);
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

            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand("UPDATE Students SET Student_Name=@Name, Email=@Email, Phone_Number=@Phone WHERE StudentId=@id", con);
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
            using var con = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            var cmd = new MySqlCommand("DELETE FROM Students WHERE StudentId=@id", con);
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }
    }
}

