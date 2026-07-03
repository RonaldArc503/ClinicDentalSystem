using FluentValidation;

namespace Identity.Application.Authentication.Commands.Register
{
    public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.")
                .MaximumLength(255);

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required.")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters.")
                .MaximumLength(30).WithMessage("Username cannot exceed 30 characters.")
                .Matches(@"^[a-zA-Z0-9._]+$").WithMessage("Username can only contain letters, digits, underscores, and periods.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .MaximumLength(128).WithMessage("Password cannot exceed 128 characters.")
                .Must(p => p.Any(char.IsUpper)).WithMessage("Password must contain at least one uppercase letter.")
                .Must(p => p.Any(char.IsLower)).WithMessage("Password must contain at least one lowercase letter.")
                .Must(p => p.Any(char.IsDigit)).WithMessage("Password must contain at least one digit.")
                .Must(p => p.Any(c => !char.IsLetterOrDigit(c))).WithMessage("Password must contain at least one special character.");
        }
    }
}
