using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using PCG_ENTITIES.PCG.Session;
using PCG_FDF.Data.DataAccess;
using Newtonsoft.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using PCG_ENTITIES.Resultsets;
using PCG_ENTITIES.PCG_FDF.UtilityEntities;
using MudBlazor;
using PCG_FDF.Data.Localization;

namespace PCG_FDF.Data.ComponentDI.AuthManagement
{
    public class AuthService : IAuthService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly ILocalStorageService _localStorage;
        private readonly PCG_FDF_DB _dataAccessService;
        private readonly WhiteLabelManager _whiteLabelManager;
        private readonly AuthenticationStateProvider _authProvider;
        private readonly NavigationManager _navigationManager;
        private readonly ApplicationState _applicationState;
        private readonly ISnackbar _snackbar;
        private readonly GlobalLocalizer _localizeService;

        private DateTime sessionExpiryTime { get; set; } = DateTime.UtcNow.AddMinutes(120);
        private int sessionTimeoutDuration { get; set; }
        private Timer? sessionTimer { get; set; }

        // Action that triggers a StateHasChanged event to rerender the component
        public event Action OnChange;

        public AuthService(HttpClient httpClient,
                           AuthenticationStateProvider authenticationStateProvider,
                           ILocalStorageService localStorage, PCG_FDF_DB data_access, WhiteLabelManager whiteLabelManager, AuthenticationStateProvider authProvider, NavigationManager navigationManager, ApplicationState applicationState
                           , GlobalLocalizer localizeService, ISnackbar snackbar
            )
        {
            _httpClient = httpClient;
            _authenticationStateProvider = authenticationStateProvider;
            _localStorage = localStorage;
            _dataAccessService = data_access;
            _whiteLabelManager = whiteLabelManager;
            _authProvider = authProvider;
            _navigationManager = navigationManager;
            _applicationState = applicationState;
            Task.Run(async () => await Refresh());
            _localizeService = localizeService;
            _snackbar = snackbar;
        }

        /// <summary>
        /// Dispara el evento de StateHasChanged y notifica al componente principal
        /// </summary>
        private void NotifyStateChanged() => OnChange?.Invoke();

        public async Task<RegisterResult> Register(RegisterModel registerModel)
        {
            // TODO: IMPLEMENT ON DATA ACCESS
            //var result = await _httpClient.PostAsJsonAsync<RegisterResult>("api/accounts", registerModel);

            return null;
        }

        public async Task<LoginResult> Login(LoginModel loginModel)
        {
            var response = await _dataAccessService.PostUserLogin(loginModel);

            var loginResult = JsonConvert.DeserializeObject<LoginResult>(await response.Content.ReadAsStringAsync());

            if (!response.IsSuccessStatusCode)
            {
                return loginResult!;
            }

            await _applicationState.SetLocations(loginResult!.Localidades);
            await _applicationState.SetCurrentLocation(loginResult.Localidad_Predeterminada);
            sessionExpiryTime = loginResult.Refresh_Expiry;
            sessionTimeoutDuration = CalculateSessionTimeoutDuration();
            sessionTimer = new Timer(OnSessionExpiredCallback, null, sessionTimeoutDuration, Timeout.Infinite);

            await _localStorage.SetItemAsync("authToken", loginResult!.Token);
            ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(loginModel.User);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", loginResult.Token);

            var user_context = await ((ApiAuthenticationStateProvider)_authProvider).GetAuthenticationStateAsync();
            _applicationState.SetPermissions(JsonConvert.DeserializeObject<HashSet<string>>(user_context.User.Claims.FirstOrDefault(claims => claims.Type == "Permissions")!.Value)!);
            _applicationState.SetPrincipalClient(Convert.ToInt32(user_context.User.Claims.FirstOrDefault(claims => claims.Type == "Client ID")!.Value));

            return loginResult;
        }

        private int CalculateSessionTimeoutDuration()
        {
            var currentTime = DateTime.UtcNow;
            var timeUntilExpiry = sessionExpiryTime - currentTime;
            return (int)timeUntilExpiry.TotalMilliseconds;
        }

        public async Task<bool> Refresh()
        {
            var userContext = await ((ApiAuthenticationStateProvider)_authProvider).GetAuthenticationStateAsync();
            if (userContext.User.Identity is not null && userContext.User.Identity.IsAuthenticated)
            {
                var jwt = await _localStorage.GetItemAsStringAsync("authToken");
                if (jwt is not null)
                {
                    jwt = jwt.Substring(1, jwt.Length - 2);
                    var userid = Convert.ToInt32(userContext.User.Claims.FirstOrDefault(claims => claims.Type == "User ID")!.Value);
                    var username = userContext.User.Claims.FirstOrDefault(claims => claims.Type == ClaimTypes.Name)!.Value;
                    var response = await _dataAccessService.PostRefreshLogin(new RefreshModel()
                    {
                        UserId = userid,
                        UserName = username,
                        Endpoint = _whiteLabelManager.Current_Page,
                        JWT = jwt
                    });

                    var refreshResult = JsonConvert.DeserializeObject<RefreshResult>(await response.Content.ReadAsStringAsync());

                    if (!response.IsSuccessStatusCode)
                    {
                        await Logout();
                        return false;
                    }
                    else
                    {
                        await _applicationState.SetUserAuthenticated(true);
                        sessionExpiryTime = refreshResult!.Result.Refresh_Expires;
                        sessionTimeoutDuration = CalculateSessionTimeoutDuration();
                        if (sessionTimer is null)
                        {
                            sessionTimer = new Timer(OnSessionExpiredCallback, null, sessionTimeoutDuration, Timeout.Infinite);
                        }
                        else
                        {
                            sessionTimer.Change(sessionTimeoutDuration, Timeout.Infinite);
                        }

                        await _localStorage.SetItemAsync("authToken", refreshResult!.Result.Token);
                        ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(username);
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", refreshResult.Result.Token);
                        var locations_result = await _dataAccessService.PostGetLocalities(new KeyValueInt() { Key = userid, Value = _applicationState.GetCurrentClientID()!.Value });
                        if (locations_result.Operation_Succeeded)
                        {
                            await _applicationState.SetLocations(locations_result.Result.Localidades);
                            await _applicationState.SetCurrentLocation(locations_result.Result.Localidad_Predeterminada);
                            var clients = _applicationState.GetAvailable_Subclients();
                            clients[_applicationState.GetCurrentClientID()!.Value].Locations = locations_result.Result.Client_Locations;
                            await _applicationState.SetClients(clients);
                        }
                        var subclientsResult = await _dataAccessService.PostGetSubclients(userid);
                        if (subclientsResult.Operation_Succeeded)
                        {
                            await _applicationState.SetClients(subclientsResult.Result);
                        }

                        userContext = await ((ApiAuthenticationStateProvider)_authProvider).GetAuthenticationStateAsync();
                        _applicationState.SetPermissions(JsonConvert.DeserializeObject<HashSet<string>>(userContext.User.Claims.FirstOrDefault(claims => claims.Type == "Permissions")!.Value)!);
                        _applicationState.SetPrincipalClient(Convert.ToInt32(userContext.User.Claims.FirstOrDefault(claims => claims.Type == "Client ID")!.Value));

                        NotifyStateChanged();

                        return true;
                    }
                }
            }
            return false;
        }

        private void OnSessionExpiredCallback(object? state) => Task.Run(async () => await Logout());

        public async Task Logout()
        {
            var userContext = await ((ApiAuthenticationStateProvider)_authProvider).GetAuthenticationStateAsync();
            if (userContext.User.Identity is not null && userContext.User.Identity.IsAuthenticated)
            {
                var userid = Convert.ToInt32(userContext.User.Claims.FirstOrDefault(claims => claims.Type == "User ID")!.Value);
                var username = userContext.User.Claims.FirstOrDefault(claims => claims.Type == ClaimTypes.Name)!.Value;
                var jwt = await _localStorage.GetItemAsStringAsync("authToken");
                jwt = jwt.Substring(1, jwt.Length - 2);
                await _localStorage.RemoveItemAsync("authToken");
                await _dataAccessService.PostRevokeLogin(new RefreshModel()
                {
                    UserId = userid,
                    UserName = username,
                    Endpoint = _whiteLabelManager.Current_Page,
                    JWT = jwt
                });
                ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
            sessionTimer?.Dispose();
            //Se limpian todos los estados para que no haya errores
            await _applicationState.Clear();
            await _applicationState.SetUserAuthenticated(false);
            _navigationManager.NavigateTo("/", true, true);
        }



        public void Dispose()
        {
            sessionTimer?.Dispose();
        }

        public bool GetAuthenticationState(AuthenticationState authState, bool requireQuotation = true)
        {

            var user = authState?.User;
            if (user?.Identity is null || !user.Identity.IsAuthenticated)
            {
                if(!requireQuotation)  _navigationManager.NavigateTo("/login");
                return false;
            }            

            if (requireQuotation && !_applicationState.HasPermission("FdF Alta Cotización"))
            {
                _snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomLeft;
                _snackbar.Add(_localizeService.Get("snackbar_error_no_perms"), Severity.Error);
                return false;
            }

            return true;
        }
        

    }
}
