using Blazored.LocalStorage;
using AIDMS.Shared.Constants.Storage;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace AIDMS.Client.Infrastructure.Authentication
{
    public class AuthenticationHeaderHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;

        public AuthenticationHeaderHandler(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if(request.Headers.Authorization?.Scheme != "Bearer")
            {
                var savedToken = await _localStorage.GetItemAsync<string>(StorageConstants.Local.AuthToken);

                if (!string.IsNullOrWhiteSpace(savedToken))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", savedToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
