using FluentValidation;
using StudentBlogg.Feature.Users;
namespace StudentBlogg.Validators;

public class UserRegistrationDtoValidator : AbstractValidator<UserRegistrationDto>
{
    public UserRegistrationDtoValidator()
    {
        RuleFor(x => x.Firstname)
            .NotEmpty().WithMessage("First name cannot be empty")
            .Length(2, 50).WithMessage("First name must be between 2 and 50 characters");
        
        RuleFor(x => x.Lastname)
            .NotEmpty().WithMessage("Last name cannot be empty")
            .Length(2, 50).WithMessage("Last name must be between 2 and 50 characters");
        
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username cannot be empty")
            .Length(3, 20).WithMessage("Username must be between 3 and 20 characters");
        
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password cannot be empty")
            .Length(6, 20).WithMessage("Password must be between 6 and 20 characters")
            .Matches("[A-Z]+").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]+").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]+").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]+").WithMessage("Password must contain at least one special character.")
            .Must(password => !password.Any(c => "æøåÆØÅ".Contains(c)))
            .WithMessage("Password contains invalid characters.");
    }
}