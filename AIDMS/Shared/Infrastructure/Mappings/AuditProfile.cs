using AutoMapper;
using AIDMS.Shared.Application.Responses.Audit;
using AIDMS.Shared.Infrastructure.Models.Audit;

namespace AIDMS.Shared.Infrastructure.Mappings
{
    public class AuditProfile : Profile
    {
        public AuditProfile()
        {
            CreateMap<AuditResponse, Audit>().ReverseMap();
        }
    }
}
