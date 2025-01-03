using System.ComponentModel.DataAnnotations;

namespace FiziixAPI.Models
{
    public class Projects
    {
        [Key]
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public string ? ProjectDescription { get; set; }
        public int CreatedBy { get; set; }
        public string ConnectionCode { get; set; }
        public DateTime CreateAt { get; set; }
        public string ? ProjectImage { get; set; }

        public ICollection<UserProjects> UserProjects { get; set; }
        public ICollection<Tasks> Tasks { get; set; }
    }
}
