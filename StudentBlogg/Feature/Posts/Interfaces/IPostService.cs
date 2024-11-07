using StudentBlogg.Common.Interfaces;

namespace StudentBlogg.Feature.Posts.Interfaces;

public interface IPostService : IBaseService<PostDto>
{
    Task<PostDto> CreatePostAsync(PostRegDto postDto);
    Task<IEnumerable<PostDto>> FindAsync(PostSearchParams searchParams);
}