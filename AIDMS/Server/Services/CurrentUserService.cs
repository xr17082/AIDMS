using AIDMS.Shared.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace AIDMS.Server.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            UserId = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            Claims = httpContextAccessor.HttpContext?.User?.Claims.AsEnumerable().Select(claim => new KeyValuePair<string, string>(claim.Type, claim.Value)).ToList();
        }

        public string UserId { get; }
        public List<KeyValuePair<string, string>> Claims { get; set; }
    }
}
