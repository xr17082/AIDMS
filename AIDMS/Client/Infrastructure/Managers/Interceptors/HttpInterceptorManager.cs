using AIDMS.Client.Infrastructure.Managers.Identity.Authentication;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Toolbelt.Blazor;

namespace AIDMS.Client.Infrastructure.Managers.Interceptors
{
    public class HttpInterceptorManager : IHttpInterceptorManager
    {
        private readonly HttpClientInterceptor _interceptor;
        private readonly IAuthenticationManager _authenticationManager;
        private readonly NavigationManager _navigationManager;
        private readonly ISnackbar _snacBack;

        public HttpInterceptorManager(HttpClientInterceptor interceptor, IAuthenticationManager authenticationManager, NavigationManager navigationManager, ISnackbar snacBack)
        {
            _interceptor = interceptor;
            _authenticationManager = authenticationManager;
            _navigationManager = navigationManager;
            _snacBack = snacBack;
        }

        public void DisposeEvent() => _interceptor.BeforeSendAsync -= InterceptBeforeHttpAsync;

        public async Task InterceptBeforeHttpAsync(object sender, HttpClientInterceptorEventArgs e)
        {
            var absPath = e.Request.RequestUri.AbsolutePath;
            if(!absPath.Contains("token") && !absPath.Contains("accounts"))
            {
                try
                {
                    var token = await _authenticationManager.TryRefreshToken();
                    if (!string.IsNullOrEmpty(token))
                    {
                        _snacBack.Add("Refreshed Token", Severity.Success);
                        e.Request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }
                }
                catch(Exception ex)
                {
                    _snacBack.Add("You are logged out", Severity.Error);
                    await _authenticationManager.Logout();
                    _navigationManager.NavigateTo("/");
                }
            }
        }

        public void RegisterEvent() => _interceptor.BeforeSendAsync += InterceptBeforeHttpAsync;
    }
}
