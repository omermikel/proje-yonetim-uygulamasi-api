using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FiziixAPI.Models
{
    public class RegisterDto
    {
        public string Name { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Username { get; set; }
        public DateTime CreateAt { get; set; }
        public string? UserPhoto { get; set; }
    }
}
