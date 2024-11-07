using Microsoft.AspNetCore.Mvc;
using StudentBlogg.Feature.Comments.Interfaces;
using StudentBlogg.Feature.Posts;
using StudentBlogg.Feature.Posts.Interfaces;

namespace StudentBlogg.Feature.Comments;

[ApiController]
[Route("api/v1/[controller]")]
public class CommentsController(ILogger<CommentsController> logger, ICommentService commentService, IPostService postService) : ControllerBase
{
    private readonly ILogger<CommentsController> _logger = logger;
    private readonly ICommentService _commentService = commentService;
    private readonly IPostService _postService = postService;

    [HttpGet("{postId:guid}/comments", Name = "GetComment")]
    public async Task<ActionResult<CommentDto>> GetComment(Guid postId)
    {
        PostDto? postDto = await _postService.GetByIdAsync(postId);
        IEnumerable<CommentDto> commentDto = await _commentService.GetPagedAsync(1,10);
        IEnumerable<CommentDto> result = commentDto.Where(x => x.PostId == postDto!.Id);
        
        return Ok(result);
    }

    [HttpGet(Name = "GetComments")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetComments(
        [FromQuery] CommentSearchParams? searchParams, 
        [FromQuery] int pageNr = 1, 
        [FromQuery] int pageSize = 10)
    {
        if (searchParams?.Content is null)
        {
            IEnumerable<CommentDto> commentDtos = await _commentService.GetPagedAsync(pageNr, pageSize);
            return Ok(commentDtos);
        }

        return Ok(await _commentService.FindAsync(searchParams));
    }

    [HttpPost("Post", Name = "PostComment")]
    public async Task<ActionResult<CommentDto>> PostComment(CommentRegDto dto,
    [FromQuery] Guid postId)
    {
        CommentDto? comment = await _commentService.AddComment(dto, postId);

        return comment is null
            ? BadRequest("Comment not created")
            : Ok(comment);
    }

    [HttpPut("{id:guid}", Name = "UpdateComment")]
    public async Task<ActionResult<CommentDto>> UpdateComment(Guid id, CommentDto dto)
    {
        _logger.LogInformation($"Attempting to Update comment with id {id}");
        CommentDto? result = await _commentService.UpdateAsync(id, dto);
        
        return result is null
            ? BadRequest("Comment not updated")
            : Ok(result);
    }

    [HttpDelete("{id:guid}", Name = "DeleteComment")]
    public async Task<ActionResult<CommentDto>> DeleteComment(Guid id)
    {
        _logger.LogInformation($"Attempting to Delete comment with id {id}");
        CommentDto? result = await _commentService.DeleteByIdAsync(id);
        
        return result is null
            ? BadRequest("Comment not deleted")
            : Ok(result);
    }
}