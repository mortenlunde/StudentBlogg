using StudentBlogg.Common.Interfaces;

namespace StudentBlogg.Feature.Comments;

public class CommentRegMapper : IMapper<Comment, CommentRegDto>
{
    public CommentRegDto MapToDto(Comment model)
    {
        return new CommentRegDto()
        {
            Content = model.Content
        };
    }

    public Comment MapToModel(CommentRegDto dto)
    {
        return new Comment()
        {
            Content = dto.Content
        };
    }
}