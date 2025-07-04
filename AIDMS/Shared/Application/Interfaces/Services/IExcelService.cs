﻿using AIDMS.Shared.Wrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace AIDMS.Shared.Application.Interfaces.Services
{
    public interface IExcelService
    {
        Task<string> ExportAsync<TData>(IEnumerable<TData> data, Dictionary<string, Func<TData, object>> mappers, string sheetName = "Sheet1");

        Task<IResult<IEnumerable<TEntity>>> ImportAsync<TEntity>(Stream data, Dictionary<string, Func<DataRow, TEntity, object>> mappers, string sheetName = "Sheet1");
    }
}
