using System.ComponentModel.DataAnnotations;

namespace FiziixAPI.Models
{
    public class Users
    {
        [Key]
        public int UserID { get; set; }
        public string Name { get; set; }
        public string Lastname { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime CreateAT { get; set; }
        public string ? UserPhoto { get; set; }

        public ICollection<UserProjects> UserProjects { get; set; }
    }
}
