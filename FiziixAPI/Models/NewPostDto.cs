namespace FiziixAPI.Models
{
    public class NewPostDto
    {
        public string content { get; set; }
        public int userID {  get; set; }
        public int projectID { get; set; }
        public string ? postImage{ get; set; }
    }
}
