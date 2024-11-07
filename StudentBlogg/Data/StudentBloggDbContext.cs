using Microsoft.EntityFrameworkCore;
using StudentBlogg.Feature;
using StudentBlogg.Feature.Comments;
using StudentBlogg.Feature.Posts;
using StudentBlogg.Feature.Users;
namespace StudentBlogg.Data;

public class StudentBloggDbContext(DbContextOptions<StudentBloggDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; init; }
    public DbSet<Post> Posts { get; init; }
    public DbSet<Comment> Comments { get; init; }
    public DbSet<Log> Logs { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(p => p.Username).IsUnique();
        });
    }
}