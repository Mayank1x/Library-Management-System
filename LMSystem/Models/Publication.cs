using System.ComponentModel.DataAnnotations;

namespace LMSystem.Models
{
    public enum PublicationType { Newspaper, Magazine }

    public class Publication
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The Title field is required.")]
        [StringLength(100)]
        public string? Title { get; set; }

        [Required(ErrorMessage = "The Publisher field is required.")]
        [StringLength(50)]
        public string? Publisher { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Published Date")]
        public DateTime PublishedDate { get; set; }

        [Required]
        public PublicationType Type { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}
