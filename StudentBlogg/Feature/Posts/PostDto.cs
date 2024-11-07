namespace StudentBlogg.Feature.Posts;

public class PostDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTime DatePosted { get; init; }
}