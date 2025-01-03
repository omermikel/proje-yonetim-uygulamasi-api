using System.ComponentModel.DataAnnotations;

namespace FiziixAPI.Models
{
    public class Posts
    {
        [Key]
        public int PostID { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserID { get; set; }
        public int ProjectID { get; set; }
        public string ? PostImage { get; set; }
    }
}
