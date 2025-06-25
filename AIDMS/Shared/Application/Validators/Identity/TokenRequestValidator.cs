using AIDMS.Shared.Application.Requests.Identity;
using FluentValidation;

namespace AIDMS.Shared.Application.Validators.Identity
{
    public class TokenRequestValidator : AbstractValidator<TokenRequest>
    {
        public TokenRequestValidator()
        {
            RuleFor(request => request.Email)
               .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage(x => "Email is required")
               .EmailAddress().WithMessage(x => "Email is not correct");
            RuleFor(request => request.Password)
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage(x => "Password is required!");
        }
    }
}
