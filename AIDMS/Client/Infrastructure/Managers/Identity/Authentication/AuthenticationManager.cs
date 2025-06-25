using Blazored.LocalStorage;
using AIDMS.Client.Infrastructure.Authentication;
using AIDMS.Client.Infrastructure.Extensions;
using AIDMS.Client.Infrastructure.Routes;
using AIDMS.Shared.Application.Requests.Identity;
using AIDMS.Shared.Application.Responses.Identity;
using AIDMS.Shared.Constants.Storage;
using AIDMS.Shared.Wrapper;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AIDMS.Client.Infrastructure.Managers.Identity.Authentication
{
    public interface IAuthenticationManager : IManager
    {
        Task<IResult> Login(TokenRequest model);

        Task<IResult> Logout();

        Task<string> RefreshToken();

        Task<string> TryRefreshToken();

        Task<string> TryForceRefreshToken();

        Task<ClaimsPrincipal> CurrentUser();
    }

    public class AuthenticationManager : IAuthenticationManager
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public AuthenticationManager(HttpClient httpClient, ILocalStorageService localStorage, AuthenticationStateProvider authenticationStateProvider)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<IResult> Login(TokenRequest model)
        {
            var response = await _httpClient.PostAsJsonAsync(TokenEndpoints.Get, model);
            var result = await response.ToResult<TokenResponse>();

            if (result.Succeeded)
            {
                var token = result.Data?.Token;
                var refreshToken = result.Data?.RefreshToken;
                var userIamgeURL = result.Data?.UserImageURL;
                await _localStorage.SetItemAsync(StorageConstants.Local.AuthToken, token);
                await _localStorage.SetItemAsync(StorageConstants.Local.RefreshToken, refreshToken);
                if (!string.IsNullOrWhiteSpace(userIamgeURL))
                    await _localStorage.SetItemAsync(StorageConstants.Local.UserImageURL, userIamgeURL);

                await ((CustomStateProvider)_authenticationStateProvider).StateChangedAsync();

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                return await Result.SuccessAsync();
            }
            else
                return await Result.FailAsync(result.Messages);
        }

        public async Task<IResult> Logout()
        {
            await _localStorage.RemoveItemAsync(StorageConstants.Local.AuthToken);
            await _localStorage.RemoveItemAsync(StorageConstants.Local.RefreshToken);
            await _localStorage.RemoveItemAsync(StorageConstants.Local.UserImageURL);
            ((CustomStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
            _httpClient.DefaultRequestHeaders.Authorization = null;
            return await Result.SuccessAsync();
        }

        public async Task<string> RefreshToken()
        {
            var token = await _localStorage.GetItemAsync<string>(StorageConstants.Local.AuthToken);
            var refreshToken = await _localStorage.GetItemAsync<string>(StorageConstants.Local.RefreshToken);

            var response = await _httpClient.PostAsJsonAsync(TokenEndpoints.Refresh, new RefreshTokenRequest { Token = token , RefreshToken = refreshToken});

            var result = await response.ToResult<TokenResponse>();

            if (!result.Succeeded)
                throw new ApplicationException("Something went wrong during the refresh token action");

            token = result.Data?.Token;
            refreshToken = result.Data?.RefreshToken;
            await _localStorage.SetItemAsync(StorageConstants.Local.AuthToken, token);
            await _localStorage.SetItemAsync(StorageConstants.Local.RefreshToken, refreshToken);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return token;
        }

        public async Task<string> TryRefreshToken()
        {
            var availableToken = await _localStorage.GetItemAsync<string>(StorageConstants.Local.RefreshToken);
            if (string.IsNullOrWhiteSpace(availableToken))
                return string.Empty;

            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            var exp = user.FindFirst(x => x.Type.Equals("exp"))?.Value;
            var expTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(exp));
            var timeUtc = DateTime.Now;
            var diff = expTime - timeUtc;
            if (diff.TotalMinutes <= 1)
                return await RefreshToken();
            return string.Empty;
        }

        public async Task<string> TryForceRefreshToken()=> await RefreshToken();

        public async Task<ClaimsPrincipal> CurrentUser()
        {
            var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
            return state.User;
        }
    }
}
