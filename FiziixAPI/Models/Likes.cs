using System.ComponentModel.DataAnnotations;

namespace FiziixAPI.Models
{
    public class Likes
    {
        [Key]
        public int LikeID { get; set; } 
        public int UserID { get; set; }
        public int PostID { get; set; }
    }
}
