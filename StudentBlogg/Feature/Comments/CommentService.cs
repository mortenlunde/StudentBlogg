using System.Linq.Expressions;
using StudentBlogg.Common.Interfaces;
using StudentBlogg.Feature.Comments.Interfaces;
using StudentBlogg.Feature.Users;
using StudentBlogg.Feature.Users.Interfaces;
using StudentBlogg.Middleware;
namespace StudentBlogg.Feature.Comments;

public class CommentService(
    ILogger<CommentService> logger,
    IMapper<Comment, CommentDto> mapper,
    ICommentRepository commentRepository,
    IUserRepository userRepository,
    IMapper<Comment, CommentRegDto> commentMapper,
    IHttpContextAccessor httpContextAccessor)
    : ICommentService
{
    public async Task<CommentDto?> GetByIdAsync(Guid id)
    {
        Comment? comment = await commentRepository.GetByIdAsync(id);
        
        return comment is null
            ? null
            : mapper.MapToDto(comment);
    }

    public async Task<IEnumerable<CommentDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        IEnumerable<Comment> comments = await commentRepository.GetPagedAsync(pageNumber, pageSize);
        return comments.Select(mapper.MapToDto).ToList();
    }

    public async Task<CommentDto?> AddAsync(CommentDto entity)
    {
        Comment commentModel = mapper.MapToModel(entity);
        Comment? commentResponse = await commentRepository.AddAsync(commentModel);
        
        return commentResponse is null
            ? null
            : mapper.MapToDto(commentResponse);
    }

    public async Task<CommentDto?> UpdateAsync(Guid id, CommentDto entity)
    {
        string? loggedInUserId = httpContextAccessor.HttpContext?.Items["UserId"] as string;
        
        if (!Guid.TryParse(loggedInUserId, out Guid userId))
        {
            logger.LogWarning("Invalid user id format: {UserId}", userId);
            return null;
        }
        
        User? loggedInUser = (await userRepository.FindAsync(user => user.Id == userId)).FirstOrDefault();
        if (loggedInUser is null)
        {
            logger.LogWarning("User id not found: {UserId}", userId);
            throw new NoUserFoundException($"{id}");
        }
        
        Comment? commentToUpdate = (await commentRepository.FindAsync(c => c.Id == id)).FirstOrDefault();
        if (commentToUpdate is null)
        {
            logger.LogWarning("Comment not found: {Id}", id);
            throw new CommentNotFoundException();
        }

        if (commentToUpdate.UserId != loggedInUser.Id) throw new WrongUserLoggedInException();
        
        commentToUpdate.Content = entity.Content;
        Comment? updatedComment = await commentRepository.UpdateByIdAsync(id, commentToUpdate);
            
        return updatedComment is null
            ? null
            : mapper.MapToDto(updatedComment);
    }

    public async Task<CommentDto?> DeleteByIdAsync(Guid id)
    {
        string? loggedInUserId = httpContextAccessor.HttpContext?.Items["UserId"] as string;
        if (!Guid.TryParse(loggedInUserId, out Guid loggedInUserGuid))
        {
            logger.LogWarning("Invalid UserId format: {UserId}", loggedInUserId);
            return null;
        }
        
        User? loggedInUser = (await userRepository.FindAsync(user => user.Id == loggedInUserGuid)).FirstOrDefault();
        if (loggedInUser == null)
        {
            logger.LogWarning("User {UserId} not found", loggedInUserId);
            throw new NoUserFoundException($"{id}");
        }
        
        IEnumerable<Comment> comments = await commentRepository.FindAsync(c => c.Id == id);
        Comment? commentToDelete = comments.FirstOrDefault();

        if (commentToDelete is null)
        {
            logger.LogWarning("Comment not found: {Id}", id);
            throw new CommentNotFoundException();
        }

        if (commentToDelete.UserId == loggedInUser.Id || loggedInUser.IsAdminUser)
        {
            logger.LogInformation("Deleting comment with id {commentID} for user {userID}", id, loggedInUserId);
            Comment? deletedComment = await commentRepository.DeleteByIdAsync(id);

            if (deletedComment is null)
            {
                logger.LogWarning("Comment with id {commentID} not found", id);
                return null;
            }
            
            return mapper.MapToDto(deletedComment);
        }
        logger.LogWarning("User {UserId} not autorized to delete comment with id {commentId}", loggedInUserId, id);
        throw new UnauthorisedOperation();
    }

    public async Task<CommentDto?> AddComment(CommentRegDto regDto, Guid postId)
    {
        Comment comment = commentMapper.MapToModel(regDto);
        comment.Id = Guid.NewGuid();
        if (httpContextAccessor.HttpContext?.Items["UserId"] is string userIdStr && Guid.TryParse(userIdStr, out Guid userId))
        {
            comment.UserId = userId;
        }
        else
        {
            logger.LogWarning("Invalid or missing UserId in HttpContext.");
            throw new MissingUsernameOrPasswordException();
        }

        comment.DateCommented = DateTime.UtcNow;
        comment.PostId = postId;
        
        Comment? commentResponse = await commentRepository.AddAsync(comment);
        return commentResponse is null
            ? null
            : mapper.MapToDto(commentResponse);
    }

    public async Task<IEnumerable<CommentDto>> FindAsync(CommentSearchParams searchParams)
    {
        Expression<Func<Comment, bool>> predicate = comment =>
            string.IsNullOrEmpty(searchParams.Content) || comment.Content!.Contains(searchParams.Content);

        IEnumerable<Comment> comments = await commentRepository.FindAsync(predicate);

        return comments.Select(mapper.MapToDto);
    }
}