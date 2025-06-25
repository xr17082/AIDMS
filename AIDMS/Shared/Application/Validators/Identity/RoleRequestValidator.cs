using AIDMS.Shared.Application.Requests.Identity;
using FluentValidation;

namespace AIDMS.Shared.Application.Validators.Identity
{
    public class RoleRequestValidator : AbstractValidator<RoleRequest>
    {
        public RoleRequestValidator()
        {
            RuleFor(request => request.Name)
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage(x => "Name is required");
        }
    }
}
