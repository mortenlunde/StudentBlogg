using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StudentBlogg.Feature.Posts;
using StudentBlogg.Feature.Users;

namespace StudentBlogg.Feature.Comments;

public class Comment
{
    [Key]
    public Guid Id { get; set; }
    
    [ForeignKey("PostID")]
    public Guid PostId { get; set; }
    
    [ForeignKey("UserID")]
    public Guid UserId { get; set; }
    
    [Required, StringLength(1000)]
    public string? Content { get; set; } = string.Empty;
    
    [Required]
    public DateTime DateCommented { get; set; }
    
    public virtual Post? Post { get; init; }
    public virtual User? User { get; init; }
}