using AIDMS.Client.Infrastructure.Extensions;
using AIDMS.Shared.Application.Responses;
using AIDMS.Shared.Wrapper;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AIDMS.Client.Infrastructure.Managers.Document
{
    public interface IDocumentManager : IManager
    {
        Task<IResult<List<SearchDocumentsResponse>>> SearchDocuments(string query);
    }

    public class DocumentManager(HttpClient _httpClient) : IDocumentManager
    {
        public async Task<IResult<List<SearchDocumentsResponse>>> SearchDocuments(string query)
        {
            var response = await _httpClient.GetAsync(Routes.DocumentRoute.SearchDocuments(query));

            return await response.ToResult<List<SearchDocumentsResponse>>();
        }
    }
}
