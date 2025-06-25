using AIDMS.Shared.Application.Responses;
using AIDMS.Shared.Wrapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIDMS.Shared.Application.Interfaces.Services
{
    public interface IDocumentProcessor
    {
        Task<Result<List<SearchDocumentsResponse>>> SearchDocuments(string query);
    }
}
