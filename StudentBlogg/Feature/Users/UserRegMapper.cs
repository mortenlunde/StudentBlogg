using StudentBlogg.Common.Interfaces;

namespace StudentBlogg.Feature.Users;

public class UserRegMapper : IMapper<User, UserRegistrationDto>
{
    public UserRegistrationDto MapToDto(User model)
    {
        return new UserRegistrationDto()
        {
            Username = model.Username,
            Firstname = model.FirstName,
            Lastname = model.LastName,
            Email = model.Email,
        };
    }

    public User MapToModel(UserRegistrationDto dto)
    {
        return new User()
        {
            Username = dto.Username!,
            FirstName = dto.Firstname!,
            LastName = dto.Lastname!,
            Email = dto.Email ?? string.Empty,
        };
    }
}