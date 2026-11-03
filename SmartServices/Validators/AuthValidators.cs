using FluentValidation;
using SmartDto;
using SmartEnums;

namespace SmartServices.Validators
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.BusinessName).MaximumLength(200);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
            RuleFor(x => x.WhatsAppNumber).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Role).NotEmpty().Must(role => RoleNames.All.Contains(role))
                .WithMessage("Role must be one of Supplier, SpazaOwner or Driver.");
        }
    }

    public class UserLoginDtoValidator : AbstractValidator<UserLoginDto>
    {
        public UserLoginDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(x => x.Password).NotEmpty().MaximumLength(128);
        }
    }

    public class PasswordChangeDtoValidator : AbstractValidator<PasswordChangeDto>
    {
        public PasswordChangeDtoValidator()
        {
            RuleFor(x => x.OldPassword).NotEmpty().MaximumLength(128);
            RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(128);
        }
    }

    public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(x => x.Token).NotEmpty().MaximumLength(512);
            RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(128);
        }
    }

    public class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
    {
        public RefreshTokenDtoValidator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty().MaximumLength(512);
        }
    }
}
