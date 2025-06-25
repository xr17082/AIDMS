using AIDMS.Shared.Application.Requests.Identity;
using FluentValidation;

namespace AIDMS.Shared.Application.Validators.Identity
{
    public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
    {
        public UpdateProfileRequestValidator()
        {
            RuleFor(request => request.FirstName)
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage(x => "First Name is required");
            RuleFor(request => request.LastName)
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage(x => "Last Name is required");
        }
    }
}
