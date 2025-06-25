using AutoMapper;
using AIDMS.Shared.Application.Responses.Identity;
using AIDMS.Shared.Infrastructure.Models.Identity;

namespace AIDMS.Shared.Infrastructure.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserResponse, ApplicationUser>().ReverseMap();
        }
    }
}
