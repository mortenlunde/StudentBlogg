using StudentBlogg.Common.Interfaces;

namespace StudentBlogg.Feature.Comments;

public class CommentMapper : IMapper<Comment, CommentDto>
{
    public CommentDto MapToDto(Comment model)
    {
        return new CommentDto()
        {
            Id = model.Id,
            PostId = model.PostId,
            UserId = model.UserId,
            Content = model.Content,
            DateCommented = model.DateCommented
        };
    }

    public Comment MapToModel(CommentDto dto)
    {
        return new Comment
        {
            Id = dto.Id,
            PostId = dto.PostId,
            UserId = dto.UserId,
            Content = dto.Content,
            DateCommented = dto.DateCommented,
        };
    }
}