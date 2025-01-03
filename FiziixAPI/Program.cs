using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FiziixAPI;
using FiziixAPI.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Azure.Core;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Veritabaný baðlantýsýný ekleyin
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/login", async (LoginDto loginDto, AppDbContext db) =>
{
    var user = await db.Users
        .FirstOrDefaultAsync(u => u.Username == loginDto.Username && u.Password == loginDto.Password);

    if (user is null)
    {
        return Results.Unauthorized(); 
    }

    var userDto = new
    {
        UserID = user.UserID,
        Name = user.Name,
        Lastname = user.Lastname,
        Phone = user.Phone,
        Email = user.Email,
        Username = user.Username,
        Password = user.Password,
        UserPhoto = user.UserPhoto

    };

    return Results.Ok(userDto);
});
app.MapGet("/homepage/{userId:int}", async (int userId, AppDbContext dbContext) =>
{
    // 1. Görevler
    var tasks = await dbContext.Tasks
        .Where(t => t.AssignedTo == userId)
        .Join(dbContext.Projects,
            t => t.ProjectID,
            p => p.ProjectID,
            (t, p) => new
            {
                ProjectName = p.ProjectName,
                TaskName = t.TaskName,
                DueDate = t.DueDate,
                TaskStatus = t.Status
            })
        .OrderBy(t => t.DueDate)
        .ToListAsync();

    // 2. Projelerdeki Tamamlanmýþ ve Tamamlanmamýþ Görev Sayýsý
    var projectTasksStatus = await dbContext.Projects
        .Where(p => p.UserProjects.Any(up => up.UserID == userId))
        .GroupJoin(dbContext.Tasks,
            p => p.ProjectID,
            t => t.ProjectID,
            (p, tasks) => new { Project = p, Tasks = tasks })
        .Select(g => new
        {
            ProjectName = g.Project.ProjectName,
            CompletedTasks = g.Tasks.Count(t => t.AssignedTo == userId && t.Status == 2),
            IncompleteTasks = g.Tasks.Count(t => t.AssignedTo == userId && t.Status != 2)
        })
        .ToListAsync();

    // 3. Kullanýcýnýn Görevle Ýlgili Beðenileri
    var userLikes = await dbContext.Projects
        .Where(p => dbContext.Posts.Any(post => post.ProjectID == p.ProjectID && post.UserID == userId))
        .Select(p => new
        {
            ProjectName = p.ProjectName,
            LikeCount = dbContext.Likes.Count(l => dbContext.Posts.Any(post => post.ProjectID == p.ProjectID && post.UserID == userId && post.PostID == l.PostID))
        })
        .ToListAsync();

    // 4. Kullanýcýnýn Projelerindeki Gönderi Sayýsý
    var userPosts = await dbContext.UserProjects
        .Where(up => up.UserID == userId)
        .Select(up => new
        {
            ProjectName = dbContext.Projects.FirstOrDefault(p => p.ProjectID == up.ProjectID).ProjectName,
            PostCount = dbContext.Posts.Count(p => p.ProjectID == up.ProjectID && p.UserID == userId)
        })
        .ToListAsync();

    // Tüm verileri birleþtir
    var dashboardData = new
    {
        Tasks = tasks,
        ProjectTasksStatus = projectTasksStatus,
        UserLikes = userLikes,
        UserPosts = userPosts
    };

    return Results.Ok(dashboardData);
});

//app.MapGet("/tasks/{userId:int}", async (int userId, AppDbContext dbContext) =>
//{
//    var tasks = await dbContext.Tasks
//        .Where(t => t.AssignedTo == userId)
//        .Join(dbContext.Projects,
//            t => t.ProjectID,
//            p => p.ProjectID,
//            (t, p) => new
//            {
//                ProjectName = p.ProjectName,
//                TaskName = t.TaskName,
//                DueDate = t.DueDate,
//                TaskStatus = t.Status
//            })
//        .OrderBy(t => t.DueDate)
//        .ToListAsync();

//    return Results.Ok(tasks);
//});

//app.MapGet("/projects/tasks-status", async (int userId, AppDbContext dbContext) =>
//{
//    var result = await dbContext.Projects
//    .Where(p => p.UserProjects.Any(up => up.UserID == userId)) // Kullanýcýnýn projeleri
//    .GroupJoin(
//        dbContext.Tasks, // Görevlerle birleþtirme
//        p => p.ProjectID,
//        t => t.ProjectID,
//        (p, tasks) => new { Project = p, Tasks = tasks })
//    .Select(g => new
//    {
//        ProjectName = g.Project.ProjectName,
//        CompletedTasks = g.Tasks.Count(t => t.AssignedTo == userId && t.Status == 2),
//        IncompleteTasks = g.Tasks.Count(t => t.AssignedTo == userId && t.Status != 2)
//    })
//    .ToListAsync();

//    return result;
//});
//app.MapGet("/user/{userId:int}/projects/likes", async (int userId, AppDbContext dbContext) =>
//{
//    var result = await dbContext.Projects
//        .Where(p => dbContext.Posts.Any(post => post.ProjectID == p.ProjectID && post.UserID == userId))
//        .Select(p => new
//        {
//            ProjectName = p.ProjectName,
//            LikeCount = dbContext.Likes.Count(l => dbContext.Posts.Any(post => post.ProjectID == p.ProjectID && post.UserID == userId && post.PostID == l.PostID))
//        })
//        .ToListAsync();

//    return Results.Ok(result);
//});

//app.MapGet("/user/{userId:int}/projects/posts", async (int userId, AppDbContext dbContext) =>
//{
//    var result = await dbContext.UserProjects
//        .Where(up => up.UserID == userId)
//        .Select(up => new
//        {
//            ProjectName = dbContext.Projects.FirstOrDefault(p => p.ProjectID == up.ProjectID).ProjectName,
//            PostCount = dbContext.Posts.Count(p => p.ProjectID == up.ProjectID && p.UserID == userId)
//        })
//        .ToListAsync();

//    return Results.Ok(result);
//});
app.MapGet("/users/checkphone/{phone}", async(string phone, AppDbContext dbContext)=>
{
    var user = await dbContext.Users.CountAsync(u => u.Phone==phone);

    if (user > 0)
        return Results.BadRequest();
    else
        return Results.Ok();
});

app.MapGet("/users/checkmail/{mail}", async (string mail, AppDbContext dbContext) =>
{
    var user = await dbContext.Users.CountAsync(u => u.Email==mail);

    if (user > 0)
        return Results.BadRequest();
    else
        return Results.Ok();
});

app.MapGet("/users/checkusername/{username}", async (string username, AppDbContext dbContext) =>
{
    var user = await dbContext.Users.CountAsync(u => u.Username == username);

    if (user > 0)
        return Results.BadRequest();
    else
        return Results.Ok();
});

app.MapPost("users/register", async(RegisterDto register ,AppDbContext dbContext) =>
{
    var user = new Users()
    {
        Name = register.Name,
        Lastname = register.Lastname,
        Phone = register.Phone,
        Email = register.Email,
        Username = register.Username,
        Password = register.Password,
        CreateAT = DateTime.Now,  // Users sýnýfýnda CreateAT
        UserPhoto = null
    };
    try
    {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return Results.Ok();
    }
    catch
    {
        return Results.BadRequest();
    }
    
});

app.MapPost("/users/forgotpassword", async(ForgotPassword password, AppDbContext dbContext) =>
{
    var user = await dbContext.Users
        .FirstOrDefaultAsync(u => u.Username == password.Username && u.Phone == password.Phone);

    if (user is null)
    {
        return Results.NotFound("User not found.");
    }

    return Results.Ok(user.Password);
});


app.MapGet("/users/{id:int}", async (int id, AppDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null)
    {
        return Results.NotFound($"User with ID {id} not found.");
    }
    return Results.Ok(user);
});

app.MapGet("/users/{username}", async (string username, AppDbContext db) =>
{
    var user = await db.Users
        .Where(u => u.Username == username)
        .Select(u => new { u.UserID })
        .FirstOrDefaultAsync();

    if (user == null)
    {
        return Results.NotFound("Kullanýcý bulunamadý.");
    }

    return Results.Ok(user);
});

app.MapGet("/users/{userid:int}/tasks", async(int userid, AppDbContext dbContext)=>
{
    var tasks = await dbContext.Tasks
        .Where(t => t.AssignedTo == userid)
        .Select(t => new Tasks
        {
            TaskID = t.TaskID,
            ProjectID = t.ProjectID,
            TaskName = t.TaskName,
            TaskDescription = t.TaskDescription,
            Status = t.Status,  // Modify based on your database type
            CreatedBy = t.CreatedBy,
            AssignedTo = t.AssignedTo,
            CreateAt = t.CreateAt,
            DueDate = t.DueDate,
            File = t.File
        }).ToListAsync();

    if (tasks == null || !tasks.Any())
    {
        return Results.NotFound("No tasks found for this project.");
    }

    return Results.Ok(tasks);
});

app.MapPut("/users/{userid:int}/tasks/{taskid:int}/addfile", async (AddFileDto addfile, int userid, int taskid, AppDbContext dbContext) =>
{
    if (addfile.TaskID < 0)
        return Results.BadRequest();

    try
    {
        var task = await dbContext.Tasks.FindAsync(addfile.TaskID);
        if (task == null)
        {
            return Results.NotFound("Task not found.");
        }

        // Güncelleme iþlemleri
        task.Status = 1;
        task.File = addfile.File; // [File] sütununu güncelleyin (ProjectImage alaný örnektir)

        dbContext.Tasks.Update(task);
        await dbContext.SaveChangesAsync();

        return Results.Ok("Task updated successfully.");
    }
    catch (Exception ex)
    {
        return Results.Problem($"An error occurred: {ex.Message}");
    }

});

app.MapPut("/users/updateprofile", async (UpdateProfileDto updateProfile, AppDbContext dbContext) =>
{
    var user = await dbContext.Users.FindAsync(updateProfile.UserId);
    if (user == null)
    {
        return Results.NotFound("User not found.");
    }

    if (updateProfile.Username != user.Username)
    {
        var usernameExists = await dbContext.Users.AnyAsync(u => u.Username == updateProfile.Username);
        if (usernameExists)
        {
            return Results.BadRequest("This username is already in use.");
        }
    }

    user.Name = updateProfile.Name;
    user.Lastname = updateProfile.Lastname;
    user.Phone = updateProfile.Phone;
    user.Email = updateProfile.Email;
    user.Username = updateProfile.Username;

    await dbContext.SaveChangesAsync();

    return Results.Ok("Your profile has been updated.");
});

app.MapPut("/users/changepassword", async (ChangePasswordDto changepassword, AppDbContext dbContext) =>
{
    var user = await dbContext.Users.FindAsync(changepassword.Userid);
    if (user == null)
    {
        return Results.NotFound("User not found.");
    }

    user.Password = changepassword.Password;
    

    await dbContext.SaveChangesAsync();
    return Results.Ok("Password has changed");
});

app.MapPut("/users/changephoto", async(ChangePhotoDto changephoto, AppDbContext dbContext) =>
{
    var user = await dbContext.Users.FindAsync(changephoto.Userid);
    if(user == null)
    {
        return Results.NotFound("User not found.");
    }

    user.UserPhoto = changephoto.UserPhoto;

    await dbContext.SaveChangesAsync();
    return Results.Ok("Photo has hanged!");
});

app.MapGet("/projects/{userId:int}", async (int userId, AppDbContext db) =>
{
    // Kullanýcýnýn dahil olduðu proje ID'lerini al
    var userProjects = await db.UserProjects
        .Where(up => up.UserID == userId)
        .Select(up => up.ProjectID)
        .ToListAsync();

    // Bu projelere ait bilgileri al
    var projects = await db.Projects
        .Where(p => userProjects.Contains(p.ProjectID))
        .Select(p => new
        {
            p.ProjectID,
            p.ProjectName,
            ProjectDescription = p.ProjectDescription ?? string.Empty, // NULL deðerler için varsayýlan deðer
            ProjectImage = p.ProjectImage ?? string.Empty, // NULL deðerler için varsayýlan deðer
            Manager = db.Users
                        .Where(u => u.UserID == p.CreatedBy)
                        .Select(u => u.Username)
                        .FirstOrDefault() ?? "Unknown", // NULL deðerler için varsayýlan deðer
            MemberCount = db.UserProjects
                            .Where(up => up.ProjectID == p.ProjectID)
                            .Count()
        })
        .ToListAsync();

    return Results.Ok(projects);
});

app.MapGet("/projects/code/{connectionCode}", async (string connectionCode, AppDbContext dbContext) =>
{
    var count = await dbContext.Projects
        .Where(p => p.ConnectionCode == connectionCode)
        .CountAsync();

    return Results.Ok(count);
});

app.MapPost("/projects/newproject", async (NewProjectDto newproject, AppDbContext dbContext) =>
{
    var project = new Projects
    {
        ProjectName = newproject.projectName,
        ProjectDescription = string.IsNullOrEmpty(newproject.projectDescription) ? null : newproject.projectDescription,
        CreatedBy = newproject.createdBy,
        CreateAt = DateTime.Now,
        ConnectionCode = newproject.connectionCode,
        ProjectImage = string.IsNullOrEmpty(newproject.projectImage) ? null : newproject.projectImage
    };

    try
    {
        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync();

        var userProject = new UserProjects
        {
            UserID = newproject.createdBy,
            ProjectID = project.ProjectID,
            Position = 0
        };

        dbContext.UserProjects.Add(userProject);
        await dbContext.SaveChangesAsync();

        return Results.Ok("*Project has been created!");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Veritabanýna veri eklenirken bir hata oluþtu: {ex.Message}");
    }
});

app.MapPost("/projects/joinproject", async (JoinProjectDto joinproject, AppDbContext dbContext) =>
{
    var code = joinproject.ConnectionCode;

    var project = await dbContext.Projects
        .FirstOrDefaultAsync(p => p.ConnectionCode == code);

    if (project == null)
    {
        return Results.BadRequest(new { message = "*Proje bulunamadý!" });
    }

    bool isUserInProject = await dbContext.UserProjects
        .AnyAsync(up => up.UserID == joinproject.UserID && up.ProjectID == project.ProjectID);

    if (isUserInProject)
    {
        return Results.BadRequest(new { message = "*Bu projeye zaten katýldýnýz!" });
    }

    var join = new UserProjects
    {
        UserID = joinproject.UserID,
        ProjectID = project.ProjectID,
        Position = 1
    };

    dbContext.UserProjects.Add(join);
    await dbContext.SaveChangesAsync();

    return Results.Ok(new { message = "Projeye baþarýyla katýldýnýz!" });
});

app.MapGet("/posts/{userid:int}/{offset:int}", async(int userid,int offset, AppDbContext dbContext)=>
{
    var posts = await dbContext.Posts
        .Where(p => dbContext.UserProjects.Any(up => up.UserID == userid && up.ProjectID == p.ProjectID))
        .OrderByDescending(p => p.CreatedAt)
        .Skip(offset)
        .Take(3)
        .Select(p => new PostsDto
        {
            PostID = p.PostID,
            ProjectName = dbContext.Projects.Where(pr => pr.ProjectID == p.ProjectID).Select(pr => pr.ProjectName).FirstOrDefault(),
            Username = dbContext.Users.Where(u => u.UserID == p.UserID).Select(u => u.Username).FirstOrDefault(),
            UserPhoto = dbContext.Users.Where(u => u.UserID == p.UserID).Select(u => u.UserPhoto).FirstOrDefault(),
            IsLiked = dbContext.Likes.Any(l => l.PostID == p.PostID && l.UserID == userid),
            Content = p.Content,
            CreatedAt = p.CreatedAt,
            PostImage = p.PostImage,
            LikeCount = dbContext.Likes.Count(l => l.PostID == p.PostID)
        })
        .ToListAsync();
    return posts;
});
app.MapPost("/posts/{userid:int}/like/{postid:int}", async (int userid, int postid, AppDbContext dbContext) =>
{
    var like = new Likes
    {
        UserID = userid,
        PostID = postid,
    };
    dbContext.Likes.Add(like);
    await dbContext.SaveChangesAsync();

    return Results.Ok();
});

app.MapDelete("/posts/{userid:int}/unlike/{postid:int}", async (int userid, int postid, AppDbContext dbContext) =>
{
    var unlike = dbContext.Likes.Where(l => l.UserID == userid && l.PostID == postid).FirstOrDefault();

    if (unlike == null)
    {
        return Results.NotFound("Like not found.");
    }

    dbContext.Likes.Remove(unlike);
    await dbContext.SaveChangesAsync();

    return Results.Ok("Successfully removed like.");
});

app.MapGet("/project/{projectId:int}", async (int projectId, AppDbContext db) =>
{
    var project = await db.Projects
        .Where(p => p.ProjectID == projectId)
        .Select(p => new
        {
            p.ProjectID,
            p.ProjectName,
            p.ProjectDescription,
            p.ProjectImage,
            p.ConnectionCode,
            Manager = db.Users.Where(u => u.UserID == p.CreatedBy).Select(u => u.Username).FirstOrDefault(),
            MemberCount = db.UserProjects.Count(up => up.ProjectID == p.ProjectID)
        })
        .FirstOrDefaultAsync();

    if (project == null)
    {
        return Results.NotFound();
    }
    

    return Results.Ok(project);
});

app.MapDelete("/project/deleteproject/{projectId:int}", async (int projectId, AppDbContext dbContext) =>
{
    using var transaction = await dbContext.Database.BeginTransactionAsync();

    try
    {
        // UserProjects tablosundan silme
        var userProjects = dbContext.UserProjects.Where(up => up.ProjectID == projectId);
        dbContext.UserProjects.RemoveRange(userProjects);

        // Likes tablosundan silme
        var postIds = dbContext.Posts.Where(p => p.ProjectID == projectId).Select(p => p.PostID);
        var likes = dbContext.Likes.Where(l => postIds.Contains(l.PostID));
        dbContext.Likes.RemoveRange(likes);

        // Posts tablosundan silme
        var posts = dbContext.Posts.Where(p => p.ProjectID == projectId);
        dbContext.Posts.RemoveRange(posts);

        // Tasks tablosundan silme
        var tasks = dbContext.Tasks.Where(t => t.ProjectID == projectId);
        dbContext.Tasks.RemoveRange(tasks);

        // Projects tablosundan silme
        var project = await dbContext.Projects.FindAsync(projectId);
        if (project != null)
        {
            dbContext.Projects.Remove(project);
        }

        // Deðiþiklikleri kaydet
        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return Results.Ok("Project and related data deleted successfully.");
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        return Results.Problem($"An error occurred: {ex.Message}");
    }
});


app.MapDelete("/project/{projectId:int}/remove/{userId:int}", async (int projectId, int userId, AppDbContext db) =>
{
    var userProject = await db.UserProjects
        .Where(up => up.UserID == userId && up.ProjectID == projectId)
        .FirstOrDefaultAsync();

    if (userProject == null)
    {
        return Results.NotFound("Kullanýcý-proje iliþkisi bulunamadý.");
    }

    db.UserProjects.Remove(userProject);
    await db.SaveChangesAsync();

    return Results.Ok(
        "Kullanýcý-proje iliþkisi baþarýyla silindi.");
});

app.MapGet("/project/{projectId:int}/users", async (int projectId, AppDbContext db) =>
{
    var users = await db.UserProjects
        .Join(db.Users,
              up => up.UserID,
              u => u.UserID,
              (up, u) => new
              {
                  u.Username,
                  u.Name,
                  u.Lastname,
                  up.Position,
                  up.ProjectID 

              })
        .Where(up => up.ProjectID == projectId)
        .ToListAsync();

    return Results.Ok(users);
});


app.MapGet("/project/{projectId:int}/posts", async (int projectId, AppDbContext dbContext) =>
{
    var posts = await dbContext.Posts
        .Where(p => p.ProjectID == projectId)
        .Join(dbContext.Users,
              p => p.UserID,
              u => u.UserID,
              (p, u) => new
              {
                  p.PostID,
                  p.Content,
                  u.Username,
                  p.CreatedAt
              })
        .OrderByDescending(p => p.CreatedAt)
        .ToListAsync();

    return Results.Ok(posts);
});

app.MapDelete("/project/{projectId:int}/deletepost/{postId:int}", async(int projectId, int postId, AppDbContext dbContext) =>
{
    var like = await dbContext.Likes
        .Where(l=> l.PostID == postId)
        .ToListAsync();

    var post = await dbContext.Posts
      .Where(t => t.PostID == postId)
      .FirstOrDefaultAsync();

    if (post == null)
    {
        return Results.NotFound("Post not found");
    }
    dbContext.Likes.RemoveRange(like);
    dbContext.Posts.Remove(post);
    await dbContext.SaveChangesAsync();

    return Results.Ok("Post deleted");
});


app.MapGet("/project/{projectId:int}/tasks", async (int projectId, AppDbContext dbContext) =>
{
    var tasks = await dbContext.Tasks
    .Where(t => t.ProjectID == projectId)
    .Join(dbContext.Users,
          task => task.AssignedTo,
          user => user.UserID,
          (task, user) => new
          {
              task.TaskID,
              AssignedUserName = user.Username, // Burada kullanýcý adýný alýyoruz
              task.TaskName,
              task.TaskDescription,
              task.Status,
              task.CreatedBy,
              //task.AssignedTo,
              task.CreateAt,
              task.DueDate,
              task.File,
              
          })
    .ToListAsync();

    return Results.Ok(tasks);
});

app.MapGet("/project/{projectId:int}/taskfile/{taskId:int}", async (int projectId, int taskId, AppDbContext dbContext) =>
{
    var file = await dbContext.Tasks
        .Where(t => t.TaskID == taskId)
        .Select(t => t.File)
        .FirstOrDefaultAsync();

    if (file == null) 
    {
        return Results.NotFound("No file found for this task.");
    }

    return Results.Ok(file);
});

app.MapPut("/project/{projectId:int}/completetask/{taskId:int}", async (int projectId, int taskId, AppDbContext dbContext) =>
{
    var task = await dbContext.Tasks.FindAsync(taskId);

    if (task == null)
    {
        return Results.NotFound("Task not found.");
    }

    task.Status = 2;

    await dbContext.SaveChangesAsync();

    return Results.Ok("Task status updated to complete.");
});

app.MapDelete("/project/{projectId:int}/deletetask/{taskId:int}", async (int projectId, int taskId, AppDbContext dbContext) =>
{
    var task = await dbContext.Tasks
       .Where(t => t.TaskID == taskId)
       .FirstOrDefaultAsync();

    if (task == null)
    {
        return Results.NotFound("Task not found");
    }

    dbContext.Tasks.Remove(task);
    await dbContext.SaveChangesAsync();

    return Results.Ok("Task deleted");
});

app.MapPost("/project/{projectId:int}/newtask", async (int projectId, NewTaskDto task, AppDbContext dbContext) =>
{
    if (task == null)
    {
        return Results.BadRequest("Invalid task data.");
    }

    try
    {
        //Kullanýcýnýn `AssignedTo` bilgisine göre UserID alýnýr
        var assignedToUserId = await dbContext.Users
            .Where(u => u.Username == task.AssignedTo)
            .Select(u => u.UserID)
            .FirstOrDefaultAsync();

        if (assignedToUserId == 0)
        {
            return Results.NotFound("Assigned user not found.");
        }

        //Yeni görev oluþtur
       var newTask = new Tasks
       {
           TaskName = task.TaskName,
           TaskDescription = task.TaskDescription,
           Status = 0,
           AssignedTo = assignedToUserId,
           CreatedBy = task.CreatedBy,
           ProjectID = projectId,
           CreateAt = DateTime.Now,
           DueDate = task.DueDate
       };

        //Görevi veritabanýna ekle
        dbContext.Tasks.Add(newTask);
        await dbContext.SaveChangesAsync();

        return Results.Created($"/tasks/{newTask.TaskID}", newTask);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapPost("/project/{projectId:int}/newpost", async (int projectId, NewPostDto post, AppDbContext dbContext) =>
{
    if (post == null)
    {
        return Results.BadRequest("Invalid post data.");
    }

    try
    {
        //Yeni görev oluþtur
        var newPost = new Posts
        {
            Content = post.content,
            UserID = post.userID,
            ProjectID = projectId,
            PostImage = string.IsNullOrEmpty(post.postImage) ? null : post.postImage,
            CreatedAt = DateTime.Now,
        };

        //Görevi veritabanýna ekle
        dbContext.Posts.Add(newPost);
        await dbContext.SaveChangesAsync();

        return Results.Created($"/posts/{newPost.PostID}", newPost);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});




app.MapGet("/", () => Showhello());

string Showhello()
{
    return "heeeeeeeee";
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
