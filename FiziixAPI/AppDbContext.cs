using FiziixAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FiziixAPI
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Users> Users { get; set; }
        public DbSet<UserProjects> UserProjects { get; set; }
        public DbSet<Projects> Projects { get; set; }
        public DbSet<Tasks> Tasks { get; set; }
        public DbSet<Posts> Posts { get; set; }
        public DbSet<Likes> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Likes>()
                .ToTable("Likes")
                .HasKey(l => l.LikeID); 

            modelBuilder.Entity<Users>()
                .ToTable("Users")
                .HasKey(l => l.UserID); 

            modelBuilder.Entity<Posts>()
                .ToTable("Posts")
                .HasKey(l => l.PostID);
            
            modelBuilder.Entity<Projects>()
                .ToTable("Projects")
                .HasKey(l=>l.ProjectID);

            modelBuilder.Entity<Tasks>()
                .ToTable("Tasks")
                .HasKey(l => l.TaskID);

            // UserProjects tablosu için birden fazla birincil anahtar tanımlama
            modelBuilder.Entity<UserProjects>()
                .ToTable("UserProjects")
                .HasKey(up => new { up.UserID, up.ProjectID });

            // User ve Project ile ilişkileri tanımlayın
            modelBuilder.Entity<UserProjects>()
                .HasOne(up => up.User)
                .WithMany(u => u.UserProjects)
                .HasForeignKey(up => up.UserID);

            modelBuilder.Entity<UserProjects>()
                .HasOne(up => up.Project)
                .WithMany(p => p.UserProjects)
                .HasForeignKey(up => up.ProjectID);

            // UserProjects tablosundaki ilişkileri doğru bir şekilde yapılandırma
            modelBuilder.Entity<UserProjects>()
                .HasKey(up => new { up.UserID, up.ProjectID }); // Composite key
            modelBuilder.Entity<UserProjects>()
                .HasOne(up => up.User)
                .WithMany(u => u.UserProjects)
                .HasForeignKey(up => up.UserID);
            modelBuilder.Entity<UserProjects>()
                .HasOne(up => up.Project)
                .WithMany(p => p.UserProjects)
                .HasForeignKey(up => up.ProjectID);
        }
    }
}
