using StudentBlogg.Common.Interfaces;

namespace StudentBlogg.Feature.Users.Interfaces;

public interface IUserService : IBaseService<UserDto>
{
    Task<UserDto?> RegisterAsync(UserRegistrationDto regDto);
    Task<User> AuthenticateUserAsync(string email, string password);
    Task<IEnumerable<UserDto>> FindAsync(UserSearchParams userSearchParams);
    Task<UserDto> UpdateRegisterAsync(Guid id, UserRegistrationDto regDto);
}