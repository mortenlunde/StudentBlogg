namespace StudentBlogg.Feature.Users;

public class UserDto
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime? Created { get; init; }
    public DateTime? Updated { get; init; }

}