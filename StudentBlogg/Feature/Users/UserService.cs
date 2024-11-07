using System.Linq.Expressions;
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
        string? loggedInUserId = httpContextAccessor.HttpContext?.Items["UserId"] as string;
        User? loggedInUser = (await userRepository.FindAsync(user => user.Id.ToString() == loggedInUserId)).FirstOrDefault();
        IEnumerable<User> userToDelete = await userRepository.FindAsync(user => user.Id == id);

        if (loggedInUser is null)
        {
            logger.LogWarning("User with ID {UserID} not found", loggedInUserId);
            throw new UnauthorizedAccessException($@"Did not find logged in user with id={id}");
        }
        
        if (!userToDelete.Any())
        {
            logger.LogWarning("User with ID {UserID} does not exist", id);
            throw new NoUserFoundException(id.ToString());
        }

        if (id.ToString().Equals(loggedInUser.Id.ToString()) || loggedInUser.IsAdminUser)
        {
            logger.LogDebug("Deleting user with ID: {UserID}", loggedInUserId);
            User? deletedUser = await userRepository.DeleteByIdAsync(id);

            if (deletedUser != null) return mapper.MapToDto(deletedUser);
            logger.LogWarning("User with ID {UserID} not found", id);
            throw new UnauthorizedAccessException($@"Did not delete user with id={id}");

        }
        throw new UnauthorisedOperation();
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

    public async Task<Guid> AuthenticateUserAsync(string username, string password)
    {
        Expression<Func<User, bool>> expression = user => user.Username == username;
        User? usr = (await userRepository.FindAsync(expression)).FirstOrDefault();
        
        if (usr is null)
            return Guid.Empty;

        return BCrypt.Net.BCrypt.Verify(password, usr.HashedPassword) ? usr.Id : Guid.Empty;
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
        string? loggedInUserId = httpContextAccessor.HttpContext?.Items["UserId"] as string;
        if (!Guid.TryParse(loggedInUserId, out Guid userId))
        {
            logger.LogWarning("Invalid user id format: {UserId}", userId);
            throw new UnauthorizedAccessException("User is not authorized.");
        }
        
        User? loggedInUser = (await userRepository.FindAsync(user => user.Id.ToString() == loggedInUserId)).FirstOrDefault();
        if (loggedInUser?.Id != id)
        {
            logger.LogWarning("Logged-in user with ID {UserId} not found.", userId);
            throw new NoUserFoundException("The specified user ID is invalid.");
        }
        if (id == Guid.Empty)
        {
            throw new NoUserFoundException("The specified user ID is invalid.");
        }
        
        User? userToUpdate = userRegistrationMapper.MapToModel(entity);
        if (userToUpdate is null)
        {
            logger.LogWarning("User with ID {UserId} not found.", userId);
            throw new NotFoundException();
        }

        if (id == loggedInUser.Id || loggedInUser.IsAdminUser)
        {
            userToUpdate.Id = id;
            userToUpdate.FirstName = entity.Firstname!;
            userToUpdate.LastName = entity.Lastname!;
            userToUpdate.Email = entity.Email!;
            userToUpdate.Username = entity.Username!;
            userToUpdate.Updated = DateTime.UtcNow;
            userToUpdate.HashedPassword = BCrypt.Net.BCrypt.HashPassword(entity.Password);
        
            User? updatedUser = await userRepository.UpdateByIdAsync(userId, userToUpdate);

            return mapper.MapToDto(updatedUser!)!;
        }
        else
        {
            logger.LogWarning("User {UserId} is not authorized to update this profile.", userId);
            throw new UnauthorizedAccessException("You are not authorized to update this profile.");
        }
    }
}