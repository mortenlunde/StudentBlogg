using System.Linq.Expressions;
using StudentBlogg.Common.Interfaces;
using StudentBlogg.Feature.Posts.Interfaces;
using StudentBlogg.Feature.Users;
using StudentBlogg.Feature.Users.Interfaces;
using StudentBlogg.Middleware;

namespace StudentBlogg.Feature.Posts;

public class PostService(ILogger<PostService> logger, IMapper<Post, PostDto> mapper, IPostRepository postRepository, 
    IUserRepository userRepository, IMapper<Post, PostRegDto> mapperReg,
    IHttpContextAccessor httpContextAccessor) : IPostService
{
    public async Task<PostDto?> GetByIdAsync(Guid id)
    {
        Post? post = await postRepository.GetByIdAsync(id);
        
        if (post?.Id is null)
            throw new PostNotFoundException();
        
        return mapper.MapToDto(post);
    }

    public async Task<IEnumerable<PostDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        IEnumerable<Post> posts = await postRepository.GetPagedAsync(pageNumber, pageSize);
        
        return posts.Select(mapper.MapToDto).ToList();
    }

    public async Task<PostDto?> AddAsync(PostDto dto)
    {
        Post postModel = mapper.MapToModel(dto);
        Post? postResponse = await postRepository.AddAsync(postModel);
        
        return postResponse is null
            ? null
            : mapper.MapToDto(postResponse);
    }

    public async Task<PostDto?> UpdateAsync(Guid id, PostDto entity)
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

        Post? postToUpdate = (await postRepository.FindAsync(post => post.Id == id)).FirstOrDefault();
        if (postToUpdate == null)
        {
            logger.LogWarning("Post with Id {PostId} not found", id);
            throw new PostNotFoundException();
        }

        if (postToUpdate.UserId == loggedInUser.Id)
        {
            postToUpdate.Title = entity.Title;
            postToUpdate.Content = entity.Content;

            Post? updatedPost = await postRepository.UpdateByIdAsync(id, postToUpdate);

            return updatedPost is null 
                ? null 
                : mapper.MapToDto(updatedPost);
        }

        throw new WrongUserLoggedInException();
    }


    public async Task<PostDto?> DeleteByIdAsync(Guid id)
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

        IEnumerable<Post> post = await postRepository.FindAsync(post => post.Id == id);
        Post? postToDelete = post.FirstOrDefault();

        if (postToDelete == null)
        {
            logger.LogWarning("Post with Id {PostId} not found", id);
            throw new PostNotFoundException();
        }

        if (postToDelete.UserId == loggedInUser.Id || loggedInUser.IsAdminUser)
        {
            logger.LogInformation("Deleting post with Id {PostId} for user {UserId}", id, loggedInUserId);
            Post? deletedPost = await postRepository.DeleteByIdAsync(id);
        
            if (deletedPost == null)
            {
                logger.LogWarning("Failed to delete post with Id {PostId}", id);
                throw new PostNotFoundException();
            }

            return mapper.MapToDto(deletedPost);
        }

        logger.LogWarning("User {UserId} is not authorized to delete post with Id {PostId}", loggedInUserId, id);
        return null;
    }


    public async Task<PostDto> CreatePostAsync(PostRegDto postDto)
    {
        Post post = mapperReg.MapToModel(postDto);
        post.Id = Guid.NewGuid();
    
        if (httpContextAccessor.HttpContext?.Items["UserId"] is string userIdString && Guid.TryParse(userIdString, out Guid userIdGuid))
        {
            post.UserId = userIdGuid;
        }
        else
        {
            throw new InvalidOperationException("UserId is not set or is not in a valid format.");
        }

        post.DatePosted = DateTime.UtcNow;
    
        Post? postResponse = await postRepository.AddAsync(post);
    
        return (postResponse is null 
            ? null 
            : mapper.MapToDto(postResponse))!;
    }

    public async Task<IEnumerable<PostDto>> FindAsync(PostSearchParams searchParams)
    {
        Expression<Func<Post, bool>> predicate = post =>
            (string.IsNullOrEmpty(searchParams.Title) || post.Title.Contains(searchParams.Title)) &&
            (string.IsNullOrEmpty(searchParams.Content) || post.Content.Contains(searchParams.Content));
        
        IEnumerable<Post> posts = await postRepository.FindAsync(predicate);
        return posts.Select(mapper.MapToDto);
    }
}