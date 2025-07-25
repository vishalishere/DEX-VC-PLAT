// © 2024 DecVCPlat. All rights reserved.

using FluentValidation;
using UserManagement.API.Models.DTOs;

namespace UserManagement.API.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required to login to DecVCPlat")
            .EmailAddress().WithMessage("Please provide a valid email address")
            .MaximumLength(254).WithMessage("Email address is too long");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required to login");
    }
}
