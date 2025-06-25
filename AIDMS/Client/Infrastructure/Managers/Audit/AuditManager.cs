using AIDMS.Client.Infrastructure.Extensions;
using AIDMS.Shared.Application.Responses.Audit;
using AIDMS.Shared.Wrapper;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AIDMS.Client.Infrastructure.Managers.Audit
{
    public interface IAuditManager : IManager
    {
        Task<IResult<IEnumerable<AuditResponse>>> GetCurrentUserTrailsAsync();

        Task<IResult<string>> DownloadFileAsync(string searchString = "", bool searchInOldValues = false, bool searchInNewValues = false);
    }

    public class AuditManager : IAuditManager
    {
        private readonly HttpClient _httpClient;

        public AuditManager(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IResult<IEnumerable<AuditResponse>>> GetCurrentUserTrailsAsync()
        {
            var response = await _httpClient.GetAsync(Routes.AuditEndpoints.GetCurrentUserTrails);
            return await response.ToResult<IEnumerable<AuditResponse>>();
        }

        public async Task<IResult<string>> DownloadFileAsync(string searchString = "", bool searchInOldValues = false, bool searchInNewValues = false)
        {
            var response = await _httpClient.GetAsync(
                string.IsNullOrWhiteSpace(searchString) ? 
                    Routes.AuditEndpoints.DownloadFile : 
                    Routes.AuditEndpoints.DownloadFileFiltered(searchString, searchInOldValues, searchInNewValues));
            return await response.ToResult<string>();
        }
    }
}
