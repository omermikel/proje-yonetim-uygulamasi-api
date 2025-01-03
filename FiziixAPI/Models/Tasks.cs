using System.ComponentModel.DataAnnotations;

namespace FiziixAPI.Models
{
    public class Tasks
    {
        [Key]
        public int TaskID { get; set; }
        public string TaskName { get; set; }
        public string TaskDescription { get; set; }
        public int Status { get; set; }
        public int CreatedBy { get; set; }
        public int AssignedTo { get; set; }
        public int ProjectID { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime DueDate { get; set; }
        public string? File { get; set; }
        //public Users AssignedUser { get; set; }

    }
}

