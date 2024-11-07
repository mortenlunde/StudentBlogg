using StudentBlogg.Common.Interfaces;

namespace StudentBlogg.Feature.Comments.Interfaces;

public interface ICommentService : IBaseService<CommentDto>
{
    Task<CommentDto?> AddComment(CommentRegDto regDto, Guid postId);
    Task<IEnumerable<CommentDto>> FindAsync(CommentSearchParams commentSearchParams);
}