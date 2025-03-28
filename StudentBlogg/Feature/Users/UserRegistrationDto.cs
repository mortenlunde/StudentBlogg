using System.ComponentModel.DataAnnotations;

namespace StudentBlogg.Feature.Users;

public class UserRegistrationDto
{
    
    public string? Username { get; set; }
    public string? Firstname { get; set; }
    public string? Lastname { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
}