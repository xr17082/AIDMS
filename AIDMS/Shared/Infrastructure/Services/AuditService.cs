using AutoMapper;
using AIDMS.Shared.Application.Extensions;
using AIDMS.Shared.Application.Interfaces.Services;
using AIDMS.Shared.Application.Responses;
using AIDMS.Shared.Application.Responses.Audit;
using AIDMS.Shared.Infrastructure.Contexts;
using AIDMS.Shared.Infrastructure.Models.Audit;
using AIDMS.Shared.Infrastructure.Specification;
using AIDMS.Shared.Wrapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AIDMS.Shared.Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IExcelService _excelService;

        public AuditService(IMapper mapper, AppDbContext context, IExcelService excelService)
        {
            _mapper = mapper;
            _context = context;
            _excelService = excelService;
        }

        public async Task<IResult<IEnumerable<AuditResponse>>> GetCurrentUserTrailsAsync(string userId)
        {
            var trails = await _context.AuditTrails.Where(a => a.UserId == userId).OrderByDescending(a => a.Id).Take(250).ToListAsync();
            var mappedLogs = _mapper.Map<List<AuditResponse>>(trails);
            return await Result<IEnumerable<AuditResponse>>.SuccessAsync(mappedLogs);
        }

        public async Task<IResult<string>> ExportToExcelAsync(string userId, string searchString = "", bool searchInOldValues = false, bool searchInNewValues = false)
        {
            var auditSpec = new AuditFilterSpecification(userId, searchString, searchInOldValues, searchInNewValues);
            var trails = await _context.AuditTrails
                .Specify(auditSpec)
                .OrderByDescending(a => a.DateTime)
                .ToListAsync();
            var data = await _excelService.ExportAsync(trails, sheetName: "Audit trails",
                mappers: new Dictionary<string, Func<Audit, object>>
                {
                    { "Table Name", item => item.TableName },
                    { "Type", item => item.Type },
                    { "Date Time (Local)", item => DateTime.SpecifyKind(item.DateTime, DateTimeKind.Utc).ToLocalTime().ToString("G", CultureInfo.CurrentCulture) },
                    { "Date Time (UTC)", item => item.DateTime.ToString("G", CultureInfo.CurrentCulture) },
                    { "Primary Key", item => item.PrimaryKey },
                    { "Old Values", item => item.OldValues },
                    { "New Values", item => item.NewValues },
                });

            return await Result<string>.SuccessAsync(data: data);
        }
    }
}
