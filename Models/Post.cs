using System.ComponentModel.DataAnnotations;

namespace Test.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}

