namespace StudentBlogg.Feature.Comments;

public class CommentDto
{
    public Guid Id { get; init; }
    public Guid PostId { get; init; }
    public Guid UserId { get; init; }
    public string? Content { get; init; } = string.Empty;
    public DateTime DateCommented { get; init; }
}