using System.ComponentModel.DataAnnotations;

namespace FiziixAPI.Models
{
    public class UserProjects
    {
        [Key]
        public int UserID { get; set; }
        public int ProjectID { get; set; }
        public int Position { get; set; }

        public Users User { get; set; }
        public Projects Project { get; set; }
        
    }
}
