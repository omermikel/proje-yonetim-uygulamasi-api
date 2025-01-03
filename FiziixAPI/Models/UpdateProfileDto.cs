namespace FiziixAPI.Models
{
    public class UpdateProfileDto
    {
        public int UserId { get; set; }
        public string Name{ get; set; }
        public string Lastname{ get; set; }
        public string Phone{ get; set; }
        public string Email{ get; set; }
        public string Username { get; set; }
    }
}
