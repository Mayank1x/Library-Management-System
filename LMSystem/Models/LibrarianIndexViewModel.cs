namespace LMSystem.Models
{
    public class LibrarianIndexViewModel
    {
        public List<LibrarianModel> Librarians { get; set; } = new();
        public string? SearchTerm { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 5;
    }
}
