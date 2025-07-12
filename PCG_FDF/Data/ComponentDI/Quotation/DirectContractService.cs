using Microsoft.AspNetCore.Components.Authorization;
using PCG_ENTITIES.PCG_FDF.UtilityEntities;
using PCG_FDF.Data.DataAccess;
using PCG_FDF.Data.ComponentDI.AuthManagement;
using PCG_FDF.Utility;
using PCG_ENTITIES.PCG_FDF.QuotationEntities;
using PCG_ENTITIES.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using PCG_ENTITIES.PCG_FDF.BookingEntities;
using PCG_FDF.Data.ComponentDI.Booking;
using PCG_FDF.Data.Localization;

namespace PCG_FDF.Data.ComponentDI.Quotation
{
    public class DirectContractService
    {
        #region INJECTIONS
        private readonly ApplicationState _appStateService;
        private readonly AuthenticationStateProvider _authProvider;
        private readonly AuthService _authService;
        private readonly BookingDataCollection _bookingService;
        private readonly PCG_FDF_DB _dataAccessService;
        private readonly GlobalLocalizer _localizeService;
        private readonly NavigationManager _navigationManager;
        private readonly QuotationDataCollection _quotationService;
        private readonly ShoppingCartContainer _shoppingCart;
        private readonly ISnackbar _snackbar;
        private readonly WhiteLabelManager _whiteLabelService;

        public DirectContractService(
            ApplicationState appStateService,
            AuthenticationStateProvider authProvider,
            AuthService authService,
            BookingDataCollection bookingService,
            PCG_FDF_DB dataAccesService,
            GlobalLocalizer localizeService,
            NavigationManager navigationManager,
            QuotationDataCollection quotationService,
            ShoppingCartContainer shoppingCart,
            ISnackbar snackbar,
            WhiteLabelManager whiteLabelService
        )
        {
            _appStateService = appStateService;
            _authProvider = authProvider;
            _authService = authService;
            _bookingService = bookingService;
            _dataAccessService = dataAccesService;
            _localizeService = localizeService;
            _navigationManager = navigationManager;
            _quotationService = quotationService;
            _shoppingCart = shoppingCart;
            _snackbar = snackbar;
            _whiteLabelService = whiteLabelService;
        }
        #endregion

        #region PROPERTYS
        private bool IsInitialized { get; set; } = false;
        private IEnumerable<KeyValueInt> Services { get; set; }
        private IDictionary<int, IEnumerable<KeyValueInt>> Packages { get; set; }
        #endregion

        #region EVENTS
        public event Action OnChange;
        #endregion

        private void NotifyStateChanged() => OnChange?.Invoke();


        public async Task InitializeQuotationForm(AuthenticationState authState)
        {
            if (!_authService.GetAuthenticationState(authState, requireQuotation: false)) return;

            // Reset quotation service
            _quotationService.ResetSelf();

            // Get data client
            var userContext = await ((ApiAuthenticationStateProvider)_authProvider).GetAuthenticationStateAsync();
            var userId = Convert.ToInt32(userContext.User.Claims.FirstOrDefault(claims => claims.Type == "User ID")!.Value);
            var clientId = _appStateService.GetCurrentClientID();

            // Create quotation cart
            _quotationService.CreateQuotationCart(_shoppingCart);

            // Get services and packages
            Services = _quotationService.GetQuotationCart().GetServiceVersionPairs();
            Packages = _quotationService.GetQuotationCart().GetPackageServiceVersionPairs();

            try
            {
                // Get quotation form
                var response = await _dataAccessService.PostGetQuotationForm(new RequestQuotationForm()
                {
                    Language = LanguageUtil.getCurrentCulture(),
                    IDUsuario = userId,
                    Location_ID = _appStateService.GetCurrentLocation(),
                    IDCliente = clientId!.Value,
                    IDClientePrincipal = _appStateService.GetPrincipalClient()!.Value,
                    Page = _whiteLabelService.Current_Page,
                    Services = Services,
                    Packages = Packages,
                });

                // Save quotation depending if has a quotation elements or not
                if (response is not null && response.Operation_Succeeded)
                    await TrySaveQuotationWithQuotationElmentsAsync(response!.Result);
                else
                    await TrySaveQuotationWithoutQuotationElementsAsync();
            }
            catch (Exception ex)
            {
            }
        }

        public int GetServicesId() => Services.First().Key;

        private async Task TrySaveQuotationWithQuotationElmentsAsync(QuotationInitializer quotation)
        {
            // Regenerate object data
            FullJSONDeserializer.RegenerateObjectData(quotation);

            // Initialize quotation
            _quotationService.Initialize(quotation, _whiteLabelService.GetPageURI(), Services, Packages);
            IsInitialized = true;

            // Set can change
            _appStateService.SetCanChange(false);

            // Get quotation Tariff
            await _quotationService.GetInvalidTariff(GetServicesId());

            await SaveQuotation();
        }

        private async Task TrySaveQuotationWithoutQuotationElementsAsync()
        {

        }


        private async Task SaveQuotation()
        {
            await _quotationService.SaveQuotation(isDirectContract: true);

            var trySaveFullQuotation = await _quotationService.SaveFullQuotation();
            if (!trySaveFullQuotation)
            {
                _snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomLeft;
                _snackbar.Add(_localizeService.Get("error_saving"), Severity.Error);
                return;
            }

            _bookingService.ResetSelf();
            _bookingService.Contracting = true;

            _bookingService.QuotationPortCapital = null;
            _bookingService.IsPortCapital = true;

            _navigationManager.NavigateTo($"/book/{_quotationService.UUID}");
        }

        public bool IsShowTicketHeader()
        {
            bool isShow = true;
            if (!_quotationService.IsServiceWithoutQuotation())
            {
                NotifyStateChanged();
                return true;
            }

            if (_quotationService.IsServiceWithoutQuotation() && _bookingService.GetIsInitialized())
            {
                isShow = _bookingService.GetBookingDataComplete();
            }

            NotifyStateChanged();
            return isShow;
        }
    }
}
