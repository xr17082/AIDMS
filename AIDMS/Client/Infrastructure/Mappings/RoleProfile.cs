using AutoMapper;
using AIDMS.Shared.Application.Requests.Identity;
using AIDMS.Shared.Application.Responses.Identity;

namespace AIDMS.Client.Infrastructure.Mappings
{
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            CreateMap<PermissionResponse, PermissionRequest>().ReverseMap();
            CreateMap<RoleClaimResponse, RoleClaimRequest>().ReverseMap();
        }
    }
}
