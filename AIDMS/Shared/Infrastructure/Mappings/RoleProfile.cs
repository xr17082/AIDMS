using AutoMapper;
using AIDMS.Shared.Application.Responses.Identity;
using AIDMS.Shared.Infrastructure.Models.Identity;

namespace AIDMS.Shared.Infrastructure.Mappings
{
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            CreateMap<RoleResponse, ApplicationRole>().ReverseMap();
        }
    }
}
