namespace StudentBlogg.Feature.Posts;

public class PostDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime DatePosted { get; set; }
}