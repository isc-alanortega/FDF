using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using PCG_ENTITIES.Constants.CimaSimplex;
using PCG_ENTITIES.PCG_FDF.CimaSimplex;
using PCG_ENTITIES.PCG_FDF.CimaSimplex.Containers;
using PCG_ENTITIES.PCG_FDF.CimaSimplex.Payments;
using PCG_FDF.Components;
using PCG_FDF.Components.Dialogs;
using PCG_FDF.Data.ComponentDI.AuthManagement;
using PCG_FDF.Data.ComponentDI.CimaSimlexUserManeuvers;
using PCG_FDF.Data.DataAccess;
using System.Security.Claims;

namespace PCG_FDF.Pages.Session
{
    public partial class UserManeuverSimplexImpo
    {
        #region INJECTS
        [Inject] private PCG_FDF_DB DataAccessService {  get; set; }
        [Inject] private CimaSimlexUserManeuverServices _cimaSimlexUserManeuverServices {  get; set; }
        [Inject] IJSRuntime jsRuntime { get; set; }
        #endregion

        #region PARAMETERS
        [Parameter]
        public string Maneuver { get; set; }
        #endregion

        #region PROPERTYS
        private GridListPager<ManeuversCustomsImpoData> PagerReference { get; set; }
        public bool IsGeneratePaymentReferece { get; set; } = false;
        private PaymentReferenceRequest ReferenceRequest { get; set; } = new();
        private IList<ManeuversCustomsImpoData>? ManeuversCustoms { get; set; }
        private int TotalManeuvers { get; set; } = 0;
        private bool IsCreditCustomer { get; set; }
        private ManeuversCustomsCimaSimplexRequest Request { get; set; } = new();
        private Dictionary<long, bool> ExpandedCardStates = new Dictionary<long, bool>();

        public bool FiltrarPorFechas { get; set; } = false;
        private string TipoFechaSeleccionada { get; set; }
        private Dictionary<int, ManeuversContainerInfo> ContainerInfoStates = new();

        #endregion

        #region BLAZOR
        protected async override Task OnInitializedAsync()
        {
            Request.Limit = 50;
            Request.Page = 1;
            cimaSimlexUserManeuverServices.OnChange += StateHasChanged;
            await GenerateBaseReferenceRequest();
            await GetManeuversAsync();
            await base.OnInitializedAsync();
        }

        private async Task GenerateBaseReferenceRequest()
        {
            var user_context = await ((ApiAuthenticationStateProvider)AuthProvider).GetAuthenticationStateAsync();
            var emailClient = user_context.User.Claims.First(claim => claim.Type == ClaimTypes.Name).Value;

            var resultClientInfo = await dataAccessService.GetClientDocumentsDataEmptyContainers();

            ReferenceRequest.AgentId = resultClientInfo.Result!.IdAgente;
            ReferenceRequest.CustomId = resultClientInfo.Result!.IdAduana;
            ReferenceRequest.Currency = 1;
            ReferenceRequest.Email = emailClient.ToString();
            ReferenceRequest.CompanyId = 2;
            ReferenceRequest.SendEmail = true;
            ReferenceRequest.Concept = 10;
            ReferenceRequest.Invoices = null;
            IsCreditCustomer = resultClientInfo.Result.Credito;
        }

        public void Dispose()
        {
            cimaSimlexUserManeuverServices.OnChange -= StateHasChanged;
        }
        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _cimaSimlexUserManeuverServices._jsDownloadService = await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./scripts/download_helper.js");
            }
            await base.OnAfterRenderAsync(firstRender);
        }
        #endregion

        private string GetColorsByStatus(string status) => ManeuverStatusConstants.GetStatusIsSuspended(status) ? " background-color: yellow;" : "";

        public async void OnClickOpenPay(ManeuversCustomsImpoData maneuverData)
        {
            IsGeneratePaymentReferece = true;
            decimal totalToPay = Convert.ToDecimal(maneuverData.Total);
            ReferenceRequest.Amount = totalToPay;
            ReferenceRequest.BookingID = maneuverData.WebRequestId;

            var response = await dataAccessService.PostPaymentReference(ReferenceRequest);
            IsGeneratePaymentReferece = false;
            StateHasChanged();
            if (response is null || !response.Operation_Succeeded || response.Result is null)
            {
                ShowMessage("Ocurrio un problema al tratar de generar la referencia de pago");
                return;
            }

            var parameters = new DialogParameters
            {
                { "PaymentReference", response.Result }
            };

            // Open dialog and add if doesn't exist
            var options = new DialogOptions
            {
                CloseButton = true,
                MaxWidth = MaxWidth.Large,
                FullWidth = true,
                DisableBackdropClick = true
            };

            var dialog = DialogService.Show<ReferencePaymentOpenPayDialog>(localizeService.Get("msg_referencespei_title"), parameters, options);

            await dialog.Result;
            await UpdateSourceAsync();
        }
        

        private void ShowMessage(string message, Severity severityType = Severity.Error)
        {
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomLeft;
            Snackbar.Add(localizeService.Get(message), severityType);
        }
        private async Task OnFiltrarPorFechasChanged(bool value)
        {
            FiltrarPorFechas = value;

            if (!FiltrarPorFechas)
            {
                Request.FromDate = null; // Clear FromDate
                Request.ToDate = null;   // Clear ToDate
                TipoFechaSeleccionada = null; // Clear selected date type
                await UpdateSourceAsync(); // Refresh the grid with cleared filters
            }

            await InvokeAsync(StateHasChanged); // Refresh the UI
        }
        #region UPDATE GRID METHODS
        private async void OnClickBtnRefresh() {
            Request.Page = 1;
            await PagerReference.UpdatePage.InvokeAsync();
        }

        private async Task UpdateSourceAsync()
        {
            ManeuversCustoms = null;
            Request.Limit = PagerReference.ItemsPerPage;
            Request.Page = PagerReference.CurrentPage;
            Request.typeDate = TipoFechaSeleccionada;
            StateHasChanged();
            await GetManeuversAsync();
        }

        private async void OnValueChange(object? value, string field)
        {
            switch (field)
            {
                case "QUERY":
                    Request.Query = value is not null ? value.ToString() : string.Empty;
                    break;
                case "FROMDATE":
                    Request.FromDate = (DateTime?)value;
                    break;
                case "TODATE":
                    Request.ToDate = (DateTime?)value;
                    break;
            }

            await UpdateSourceAsync();
        }

        private async Task GetManeuversAsync()
        {
            var response = await DataAccessService.GetManeuversCustomersCimaSimplexImpoAsync(requestGrid: Request);

            (List<ManeuversCustomsImpoData> data, int total) = (response is null || !response.Operation_Succeeded || response.Result is null)
                ? (new List<ManeuversCustomsImpoData>(), 0)
                : (response!.Result!.Data, response!.Result!.Total);

            ManeuversCustoms = data;
            TotalManeuvers = total;
            PagerReference.CheckAgainParameterSet(ManeuversCustoms);
        }
        private async Task ToggleExpandedCard(int webRequestId, string tipoOperacion)
        {
            try
            {
                if (!ContainerInfoStates.ContainsKey(webRequestId))
                {
                    var response = await DataAccessService.GetInfoContainerCimaSimplex(webRequestId, tipoOperacion);

                    if (response?.Result != null)
                        ContainerInfoStates[webRequestId] = response.Result;
                    else
                        ContainerInfoStates[webRequestId] = new(); // vacío si no trae nada
                }

                ExpandedCardStates[webRequestId] = !ExpandedCardStates.ContainsKey(webRequestId) || !ExpandedCardStates[webRequestId];

                StateHasChanged();
            }
            catch (Exception ex)
            {
                Snackbar.Add("Ocurrió un error al consultar la información.", Severity.Error);
                StateHasChanged();
            }
        }


        #endregion
    }
}