using StudentBlogg.Common.Interfaces;

namespace StudentBlogg.Feature.Posts;

public class PostMapper : IMapper<Post, PostDto>
{
    public PostDto MapToDto(Post model)
    {
        return new PostDto()
        {
            Id = model.Id,
            UserId = model.UserId,
            Title = model.Title,
            Content = model.Content,
            DatePosted = model.DatePosted
        };
    }

    public Post MapToModel(PostDto dto)
    {
        return new Post
        {
            UserId = dto.UserId,
            Title = dto.Title,
            Content = dto.Content,
            DatePosted = dto.DatePosted,
        };
    }
}