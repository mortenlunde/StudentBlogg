using Microsoft.AspNetCore.Mvc;
using StudentBlogg.Feature.Posts;
using StudentBlogg.Feature.Posts.Interfaces;
using StudentBlogg.Feature.Users.Interfaces;

namespace StudentBlogg.Feature.Users;

[ApiController]
[Route("api/v1/[controller]")]
public class UsersController(ILogger<UsersController> logger, IUserService userService, IPostService postService) : ControllerBase
{
    private readonly ILogger<UsersController> _logger = logger;
    private readonly IUserService _userService = userService;
    private readonly IPostService _postService = postService;
    
    [HttpGet("{id:guid}", Name = "GetUserByIDAsync")]
    public async Task<ActionResult<UserDto>> GetUserByIdAsync(Guid id)
    {
        UserDto? userDto = await _userService.GetByIdAsync(id);
        if (userDto == null)
            _logger.LogError($"User with ID {id} not found");
        return userDto is null 
            ? NotFound("User not found") 
            : Ok(userDto);
    }
    
    [HttpGet(Name = "GetUsersAsync")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersAsync(
        [FromQuery] UserSearchParams? searchParams, [FromQuery] int pageNr = 1, [FromQuery] int pageSize = 10)
    {
        if (searchParams?.Firstname is null && searchParams?.Lastname is null 
                                            && searchParams?.Email is null 
                                            && searchParams?.Username is null)
        {
            IEnumerable<UserDto> userDtos = await _userService.GetPagedAsync(pageNr, pageSize);
            return Ok(userDtos);
        }

        return Ok(await _userService.FindAsync(searchParams));
    }

    [HttpPost("Register", Name = "RegisterUserAsync")]
    public async Task<ActionResult<UserDto>> RegisterUserAsync(UserRegistrationDto dto)
    {
        UserDto? user = await _userService.RegisterAsync(dto);
        return user is null
            ? BadRequest("Failed to register new user")
            : Ok(user);
    }

    [HttpDelete("{id:guid}", Name = "DeleteUserAsync")]
    public async Task<ActionResult<UserDto>> DeleteUserAsync(Guid id)
    {
        _logger.LogInformation($"User with ID {id} deleted");
        UserDto? result = await _userService.DeleteByIdAsync(id);
        
        return result is null
            ? BadRequest("Failed to delete user")
            :Ok(result);
    }

    [HttpPut("{id:guid}", Name = "UpdateUserAsync")]
    public async Task<ActionResult<UserDto>> UpdateUserAsync(Guid id, UserRegistrationDto dto)
    {
        _logger.LogInformation($"Updating user with ID {id} updated");
        UserDto result = await _userService.UpdateRegisterAsync(id, dto);
        
        return Ok(result);
    }

    [HttpGet("{id:guid}/posts", Name = "GetUserPostsAsync")]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetUserPostsAsync(Guid id)
    {
        _logger.LogInformation($"User with ID {id} found");
        UserDto? user  = await _userService.GetByIdAsync(id);
        IEnumerable<PostDto> posts = await _postService.GetPagedAsync(1, 10);
        IEnumerable<PostDto> result = posts.Where(u => u.UserId == user!.Id);
        
        return Ok(result);
    }
}