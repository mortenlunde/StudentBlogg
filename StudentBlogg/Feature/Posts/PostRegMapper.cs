using StudentBlogg.Common.Interfaces;

namespace StudentBlogg.Feature.Posts;

public class PostRegMapper : IMapper<Post, PostRegDto>
{
    public PostRegDto MapToDto(Post model)
    {
        return new PostRegDto()
        {
            Title = model.Title,
            Content = model.Content
        };
    }

    public Post MapToModel(PostRegDto dto)
    {
        return new Post()
        {
            Title = dto.Title,
            Content = dto.Content
        };
    }
}