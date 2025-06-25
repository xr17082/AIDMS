using AIDMS.Shared.Application.Interfaces.Services;
using AIDMS.Shared.Application.Interfaces.Services.Account.Identity;
using AIDMS.Shared.Application.Interfaces.Services.Identity;
using AIDMS.Shared.Application.Requests.Identity;
using AIDMS.Shared.Infrastructure.Models.Identity;
using AIDMS.Shared.Wrapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AIDMS.Shared.Infrastructure.Services.Identity
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUploadService _uploadService;

        public AccountService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IUploadService uploadService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _uploadService = uploadService;
        }

        public async Task<IResult> ChangePasswordAsync(ChangePasswordRequest model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return await Result.FailAsync("User not found.");

            var identityResult = await _userManager.ChangePasswordAsync(user, model.Password, model.NewPassword);
            var errors = identityResult.Errors.Select(e=> e.Description.ToString()).ToList();
            
            if (!identityResult.Succeeded)
                return await Result.FailAsync(errors);

            return await Result.SuccessAsync();
        }

        public async Task<IResult> UpdateProfileAsync(UpdateProfileRequest request, string userId)
        {
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                var userWithSamePhone = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber);
                if (userWithSamePhone != null)
                    return await Result.FailAsync($"Phone number {request.PhoneNumber} already in use.");
            }

            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithSameEmail == null || userWithSameEmail.Id == userId)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return await Result.FailAsync("User not found");

                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.PhoneNumber = request.PhoneNumber;
                var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
                if (request.PhoneNumber != phoneNumber)
                    await _userManager.SetPhoneNumberAsync(user, request.PhoneNumber);

                var identityResult = await _userManager.UpdateAsync(user);
                var errors = identityResult.Errors.Select(e => e.Description.ToString()).ToList();
                await _signInManager.RefreshSignInAsync(user);
                if (!identityResult.Succeeded)
                    return await Result.FailAsync(errors);
                return await Result.SuccessAsync();
            }
            else
                return await Result.FailAsync($"Email {request.Email} already in use.");
        }

        public async Task<IResult<string>> GetProfilePictureAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return await Result<string>.FailAsync("User Not Found");
            }
            return await Result<string>.SuccessAsync(data: user.ProfilePictureDataUrl);
        }

        public async Task<IResult<string>> UpdateProfilePictureAsync(UpdateProfilePictureRequest request, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return await Result<string>.FailAsync(message: "User Not Found");
            var filePath = _uploadService.UploadAsync(request);
            user.ProfilePictureDataUrl = filePath;
            var identityResult = await _userManager.UpdateAsync(user);
            var errors = identityResult.Errors.Select(e =>e.Description.ToString()).ToList();
            return identityResult.Succeeded ? await Result<string>.SuccessAsync(data: filePath) : await Result<string>.FailAsync(errors);
        }
    }
}
