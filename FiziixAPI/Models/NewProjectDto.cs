namespace FiziixAPI.Models
{
    public class NewProjectDto
    {
        public string projectName{ get; set; }
        public string ? projectDescription{ get; set; }
        public int createdBy { get; set; }
        public string connectionCode{ get; set; }
        public DateTime createAt { get; set; }
        public string ? projectImage{ get; set; }
    }
}
