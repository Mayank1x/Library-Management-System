namespace LMSystem.Models
{
    public class StudentIndexViewModel
    {
        // Search Filter
        public string? SearchTerm { get; set; }

        // Pagination Tracking
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 5;

        // Data Payload
        public List<StudentModel> Students { get; set; } = new List<StudentModel>();
    }
}
