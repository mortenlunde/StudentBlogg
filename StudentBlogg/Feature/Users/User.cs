using System.ComponentModel.DataAnnotations;
using StudentBlogg.Feature.Comments;
using StudentBlogg.Feature.Posts;

namespace StudentBlogg.Feature.Users;

public class User
{
    [Key]
    public Guid Id { get; set; }
    
    [Required, MinLength(3), MaxLength(30)]
    public string Username { get; set; } = string.Empty;
    
    [Required, MinLength(3), MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required, MinLength(3), MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [StringLength(60)]
    public string HashedPassword { get; set; } = string.Empty;
    
    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public DateTime? Created { get; set; }
    
    [Required]
    public DateTime? Updated { get; set; }
    
    [Required]
    public bool IsAdminUser { get; set; }
    
    public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
    public virtual ICollection<Post> Posts { get; set; } = new HashSet<Post>();
}