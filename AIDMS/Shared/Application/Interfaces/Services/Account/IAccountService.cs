using AIDMS.Shared.Application.Requests.Identity;
using AIDMS.Shared.Wrapper;
using System.Threading.Tasks;

namespace AIDMS.Shared.Application.Interfaces.Services.Account.Identity
{
    public interface IAccountService : IService
    {
        Task<IResult> UpdateProfileAsync(UpdateProfileRequest model, string userId);

        Task<IResult> ChangePasswordAsync(ChangePasswordRequest model, string userId);

        Task<IResult<string>> GetProfilePictureAsync(string userId);

        Task<IResult<string>> UpdateProfilePictureAsync(UpdateProfilePictureRequest request, string userId);
    }
}
