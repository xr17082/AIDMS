﻿using AIDMS.Shared.Application.Responses.Audit;
using AIDMS.Shared.Wrapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIDMS.Shared.Application.Interfaces.Services
{
    public interface IAuditService
    {
        Task<IResult<IEnumerable<AuditResponse>>> GetCurrentUserTrailsAsync(string userId);

        Task<IResult<string>> ExportToExcelAsync(string userId, string searchString = "", bool searchInOldValues = false, bool searchInNewValues = false);
    }
}
