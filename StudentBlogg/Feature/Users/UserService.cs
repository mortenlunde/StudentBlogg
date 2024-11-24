using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using StudentBlogg.Common.Interfaces;
using StudentBlogg.Feature.Users.Interfaces;
using StudentBlogg.Middleware;

namespace StudentBlogg.Feature.Users;

public class UserService(
    ILogger<UserService> logger,
    IMapper<User, UserDto?> mapper,
    IUserRepository userRepository,
    IMapper<User, UserRegistrationDto> userRegistrationMapper,
    IHttpContextAccessor httpContextAccessor)
    : IUserService
{
    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        User? model = await userRepository.GetByIdAsync(id);
        if (model?.Id == null)
            throw new NoUserFoundException("No user found");
        
        return mapper.MapToDto(model);
    }

    public async Task<IEnumerable<UserDto>> GetPagedAsync(int pageNumber, int pageSize)
    {
        IEnumerable<User> users = await userRepository.GetPagedAsync(pageNumber, pageSize);
        
        return users.Select(mapper.MapToDto).ToList()!;
    }
    
    public async Task<UserDto?> AddAsync(UserDto? dto)
    {
        User model = mapper.MapToModel(dto);
        User? modelResponse = await userRepository.AddAsync(model);
        
        return modelResponse is null 
            ? null 
            : mapper.MapToDto(modelResponse);
    }

    public async Task<UserDto?> UpdateAsync(Guid id, UserDto entity)
    {
        User userModel = mapper.MapToModel(entity);
        User? userResponse = await userRepository.UpdateByIdAsync(id, userModel);
        
        return userResponse is null
            ? null
            : mapper.MapToDto(userResponse);
    }

    public async Task<UserDto?> DeleteByIdAsync(Guid id)
{
    // Extract the Authorization header
    var authorizationHeader = httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
    if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
    {
        logger.LogWarning("Missing or invalid Authorization header.");
        throw new InvalidOperationException("Authorization header is required.");
    }

    var token = authorizationHeader.Substring("Bearer ".Length).Trim();

    Guid loggedInUserIdGuid;
    try
    {
        // Validate and parse the JWT token
        var handler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourSuperSecretKeyHereIsVeryNice")) // Replace with your key
        };

        handler.ValidateToken(token, validationParameters, out var validatedToken);

        var jwtToken = (JwtSecurityToken)validatedToken;
        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out loggedInUserIdGuid))
        {
            logger.LogWarning("UserId missing or invalid in token.");
            throw new InvalidOperationException("Invalid token payload.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error parsing or validating token.");
        throw new InvalidOperationException("Failed to parse or validate token.", ex);
    }

    // Fetch the user to delete
    var userToDelete = await userRepository.FindAsync(user => user.Id == id);

    if (!userToDelete.Any())
    {
        logger.LogWarning("User with ID {UserID} does not exist", id);
        throw new NoUserFoundException($"User with ID {id} not found.");
    }

    // Check if the logged-in user is authorized
    if (id == loggedInUserIdGuid || IsAdminUser(loggedInUserIdGuid))
    {
        logger.LogDebug("Deleting user with ID: {UserID}", id);
        var deletedUser = await userRepository.DeleteByIdAsync(id);

        if (deletedUser != null)
        {
            logger.LogInformation("User with ID {UserID} successfully deleted.", id);
            return mapper.MapToDto(deletedUser);
        }

        logger.LogWarning("Failed to delete user with ID {UserID}.", id);
        throw new InvalidOperationException($"Failed to delete user with ID {id}.");
    }

    // Unauthorized operation
    logger.LogWarning("Unauthorized operation by user {UserID} to delete user {TargetID}.", loggedInUserIdGuid, id);
    throw new UnauthorizedAccessException("You are not authorized to delete this user.");
}

private bool IsAdminUser(Guid userId)
{
    // Implement logic to verify if the user is an admin
    // Example:
    var user = userRepository.FindAsync(u => u.Id == userId).Result.FirstOrDefault();
    return user?.IsAdminUser ?? false;
}


    public async Task<UserDto?> RegisterAsync(UserRegistrationDto regDto)
    {
        User user = userRegistrationMapper.MapToModel(regDto);
        user.Created = DateTime.UtcNow;
        user.Updated = DateTime.UtcNow;
        user.IsAdminUser = false;
        user.Id = Guid.NewGuid();
        user.HashedPassword = BCrypt.Net.BCrypt.HashPassword(regDto.Password);
        
        User? userResponse = await userRepository.AddAsync(user);
        return userResponse is null
            ? null
            : mapper.MapToDto(userResponse);
    }

    public async Task<User?> AuthenticateUserAsync(string username, string password)
    {
        Expression<Func<User, bool>> expression = user => user.Username == username;
        User? usr = (await userRepository.FindAsync(expression)).FirstOrDefault();
    
        if (usr is null || !BCrypt.Net.BCrypt.Verify(password, usr.HashedPassword))
            return null;

        return usr; // Return the entire user object.
    }


    public async Task<IEnumerable<UserDto>> FindAsync(UserSearchParams searchParams)
    {
        Expression<Func<User, bool>> predicate = user =>
            (string.IsNullOrEmpty(searchParams.Username) || user.Username.Contains(searchParams.Username)) &&
            (string.IsNullOrEmpty(searchParams.Firstname) || user.FirstName.Contains(searchParams.Firstname)) &&
            (string.IsNullOrEmpty(searchParams.Lastname) || user.LastName.Contains(searchParams.Lastname)) &&
            (string.IsNullOrEmpty(searchParams.Email) || user.Email.Contains(searchParams.Email));
        
        IEnumerable<User> users = await userRepository.FindAsync(predicate);
        
        return users.Select(mapper.MapToDto)!;
    }

    public async Task<UserDto> UpdateRegisterAsync(Guid id, UserRegistrationDto entity)
{
    // Extract the Authorization header
    var authorizationHeader = httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
    if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
    {
        logger.LogWarning("Missing or invalid Authorization header.");
        throw new InvalidOperationException("Authorization header is required.");
    }

    var token = authorizationHeader.Substring("Bearer ".Length).Trim();

    Guid loggedInUserIdGuid;
    try
    {
        // Validate and parse the JWT token
        var handler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false, // Set to true if you use a specific issuer
            ValidateAudience = false, // Set to true if you use a specific audience
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourSuperSecretKeyHereIsVeryNice")) // Replace with your key
        };

        handler.ValidateToken(token, validationParameters, out var validatedToken);

        var jwtToken = (JwtSecurityToken)validatedToken;
        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == "id");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out loggedInUserIdGuid))
        {
            logger.LogWarning("UserId missing or invalid in token.");
            throw new InvalidOperationException("Invalid token payload.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error parsing or validating token.");
        throw new InvalidOperationException("Failed to parse or validate token.", ex);
    }

    // Retrieve the logged-in user
    var loggedInUser = (await userRepository.FindAsync(user => user.Id == loggedInUserIdGuid)).FirstOrDefault();
    if (loggedInUser == null)
    {
        logger.LogWarning("Logged-in user {UserId} not found.", loggedInUserIdGuid);
        throw new NoUserFoundException($"User {loggedInUserIdGuid} not found.");
    }

    // Retrieve the user to update
    var userToUpdate = (await userRepository.FindAsync(user => user.Id == id)).FirstOrDefault();
    if (userToUpdate == null)
    {
        logger.LogWarning("User with ID {UserId} not found.", id);
        throw new NotFoundException();
    }

    // Check if the logged-in user has permission to update
    if (id != loggedInUser.Id && !loggedInUser.IsAdminUser)
    {
        logger.LogWarning("User {UserId} is not authorized to update User {TargetId}.", loggedInUser.Id, id);
        throw new WrongUserLoggedInException();
    }

    // Update fields only if they are provided
    if (!string.IsNullOrWhiteSpace(entity.Firstname))
        userToUpdate.FirstName = entity.Firstname;

    if (!string.IsNullOrWhiteSpace(entity.Lastname))
        userToUpdate.LastName = entity.Lastname;

    if (!string.IsNullOrWhiteSpace(entity.Email))
        userToUpdate.Email = entity.Email;

    if (!string.IsNullOrWhiteSpace(entity.Username))
        userToUpdate.Username = entity.Username;

    if (!string.IsNullOrWhiteSpace(entity.Password))
        userToUpdate.HashedPassword = BCrypt.Net.BCrypt.HashPassword(entity.Password);

    userToUpdate.Updated = DateTime.UtcNow;

    // Update the user
    var updatedUser = await userRepository.UpdateByIdAsync(id, userToUpdate);
    if (updatedUser == null)
    {
        throw new InvalidOperationException("Failed to update user.");
    }

    return mapper.MapToDto(updatedUser);
}

}