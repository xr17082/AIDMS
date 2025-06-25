using AutoMapper;
using AIDMS.Shared.Application.Interfaces.Services;
using AIDMS.Shared.Application.Interfaces.Services.Identity;
using AIDMS.Shared.Application.Requests.Identity;
using AIDMS.Shared.Application.Responses.Identity;
using AIDMS.Shared.Infrastructure.Contexts;
using AIDMS.Shared.Infrastructure.Models.Identity;
using AIDMS.Shared.Wrapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIDMS.Shared.Infrastructure.Services.Identity
{
    public class RoleClaimService : IRoleClaimService
    {
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly AppDbContext _db;

        public RoleClaimService(IMapper mapper, ICurrentUserService currentUserService, AppDbContext db)
        {
            _mapper = mapper;
            _currentUserService = currentUserService;
            _db = db;
        }

        public async Task<Result<string>> DeleteAsync(int id)
        {
            var existingRoleClaim = await _db.RoleClaims
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x=> x.Id == id);

            if (existingRoleClaim != null)
            {
                _db.RoleClaims.Remove(existingRoleClaim);
                await _db.SaveChangesAsync(_currentUserService.UserId);
                return await Result<string>.SuccessAsync($"Role Claim {existingRoleClaim.ClaimValue} for {existingRoleClaim.Role.Name} Role deleted.");
            }
            else
                return await Result<string>.FailAsync("Role Claim does not exists");
        }

        public async Task<Result<List<RoleClaimResponse>>> GetAllAsync()
        {
            var roleClaims = await _db.RoleClaims.ToListAsync();
            var roleClaimResponses = _mapper.Map<List<RoleClaimResponse>>(roleClaims);
            return await Result<List<RoleClaimResponse>>.SuccessAsync(roleClaimResponses);
        }

        public async Task<Result<List<RoleClaimResponse>>> GetAllByRoleIdAsync(string roleId)
        {
            var roleClaims = await _db.RoleClaims
                .Include(x => x.RoleId).Where(x => x.RoleId == roleId).ToListAsync();
            var roleClaimResponse = _mapper.Map<List<RoleClaimResponse>>(roleClaims);
            return await Result<List<RoleClaimResponse>>.SuccessAsync(roleClaimResponse);
        }

        public async Task<Result<RoleClaimResponse>> GetByIdAsync(int id)
        {
            var roleClaim = await _db.RoleClaims
                .SingleOrDefaultAsync(x=> x.Id == id);
            var roleClaimResponse = _mapper.Map<RoleClaimResponse>(roleClaim);
            return await Result<RoleClaimResponse>.SuccessAsync(roleClaimResponse);
        }

        public async Task<int> GetCountAsync()
        {
            return await _db.RoleClaims.CountAsync();
        }

        public async Task<Result<string>> SaveAsync(RoleClaimRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RoleId))
                return await Result<string>.FailAsync("Role is required.");

            if(request.Id == 0)
            {
                var existingRoleClaim = await _db.RoleClaims.SingleOrDefaultAsync(x => x.RoleId == request.RoleId && x.ClaimType == request.Type && x.ClaimValue == request.Value);

                if (existingRoleClaim != null)
                    return await Result<string>.FailAsync("Role Claim already exists");

                var roleClaim = _mapper.Map<ApplicationRoleClaim>(request);
                await _db.RoleClaims.AddAsync(roleClaim);
                await _db.SaveChangesAsync(_currentUserService.UserId);
                return await Result<string>.SuccessAsync($"Role Claim {request.Value} created.");
            }
            else
            {
                var existingRoleClaim = await _db.RoleClaims.Include(x=> x.Role).SingleOrDefaultAsync(x=> x.Id == request.Id);

                if (existingRoleClaim == null)
                    return await Result<string>.SuccessAsync("Role Claim does not exist.");
                else
                {
                    existingRoleClaim.ClaimType = request.Type;
                    existingRoleClaim.ClaimValue = request.Value;
                    existingRoleClaim.Group = request.Group;
                    existingRoleClaim.Description = request.Description;
                    existingRoleClaim.RoleId = request.RoleId;
                    _db.RoleClaims.Update(existingRoleClaim);
                    await _db.SaveChangesAsync(_currentUserService.UserId);
                    return await Result<string>.SuccessAsync($"Role Claim {request.Value} for Role {existingRoleClaim.Role.Name} updated.");
                }
            }
        }
    }
}
