namespace StudentBlogg.Feature.Users;

public class UserRegistrationDto
{
    public string? Username { get; init; }
    public string? Firstname { get; init; }
    public string? Lastname { get; init; }
    public string? Email { get; init; }
    public string? Password { get; set; }
}