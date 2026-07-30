using Microsoft.AspNetCore.Authorization;
using LMSystem.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace LMSystem.Controllers
{
    [Authorize]
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

            using (var connection = new MySqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                connection.Open();

                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM Students", connection))
                {
                    model.TotalStudents = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM Books", connection))
                {
                    model.TotalBooks = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM Librarians", connection))
                {
                    model.TotalLibrarians = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM BorrowRecords WHERE ReturnDate IS NULL", connection))
                {
                    model.TotalBorrowings = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM Publications", connection))
                {
                    model.TotalPublications = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            return View(model);
        }
    }
}

