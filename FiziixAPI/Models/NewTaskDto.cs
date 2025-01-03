namespace FiziixAPI.Models
{
    public class NewTaskDto
    {
        public string TaskName { get; set; }
        public string TaskDescription { get; set; }
        public string AssignedTo { get; set; } // Kullanıcı adını temsil eder
        public int CreatedBy { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime DueDate { get; set; }
    }
}
