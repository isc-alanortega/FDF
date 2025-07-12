using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using PCG_ENTITIES.PCG_CO_CM;
using PCG_ENTITIES.PCG_FDF.CimaSimplex.Payments;
using PCG_FDF.Components.Dialogs;
using PCG_FDF.Data.ComponentDI.AuthManagement;
using PCG_FDF.Data.Entities;
using PCG_FDF.Data.Localization;
using System.Security.Claims;

namespace PCG_FDF.Components.CimaSimplexPaymentsInvoices
{
    public partial class PaymentInvoiceResumen
    {
        #region PARAMETERS
        [Parameter] public List<OpenPayInvoiceData> Invoices { get; set; }
        [Parameter] public Action<bool> NavigateResumenCallback { get; set; }
        [Parameter] public Action Reset { get; set; }
        [Parameter] public PaymentInvoceRequest DataRequest { get; set; }
        #endregion

        #region INJECTIONS
        [Inject] private ISnackbar SnackbarServices { get; set; }
        [Inject] GlobalLocalizer LocalizeService { get; set; }
        [Inject] private AuthenticationStateProvider AuthProvider { get; set; }
        #endregion

        #region PROPERTIES
        private GridListPager<OpenPayInvoiceData>? PagerReference { get; set; }
        private List<PaymentConceptsResponse> PaymentConcepts { get; set; } = new();
        private int SelectedConcept { get; set; }
        private bool ShowSkeletons { get; set; } = true;
        private bool IsGeneratePaymentReferece { get; set; } = false;
        #endregion

        #region METHODS
        protected async override Task OnInitializedAsync()
        {
            var response = await dataAccessService.GetPaymentConcepts();
            ShowSkeletons = false;

            if (response is null || !response.Operation_Succeeded || response.Result is null)
            {
                return;
            }

            PaymentConcepts = response.Result.ToList();
            SelectedConcept = PaymentConcepts.First(concept => concept.Id == 1).Id;
        }

        private void PaymentConceptChanged(int conceptId) => SelectedConcept = conceptId;

        private decimal GetTotalAmount() => Math.Round(Invoices.Sum(f => f.Payment_Amount), 2);

        private async void GeneretePaymentReference()
        {
            if (Invoices == null || !Invoices.Any())
            {
                ShowMessage(localize.Get("error_save_voucherpayment"), Severity.Error);
                return;
            }

            if (IsGeneratePaymentReferece) return;

            IsGeneratePaymentReferece = true;
            StateHasChanged();

            var user_context = await ((ApiAuthenticationStateProvider)AuthProvider).GetAuthenticationStateAsync();
            var emailClient = user_context.User.Claims.First(claim => claim.Type == ClaimTypes.Name).Value;
            var genereteRequest = new PaymentReferenceRequest()
            {
                Amount = GetTotalAmount(),
                Currency = DataRequest.CurrencyId,
                Concept = SelectedConcept,
                AgentId = DataRequest.AgentId,
                CompanyId = 2,
                CustomId = DataRequest.CustomId,
                SendEmail = true,
                Email = emailClient.ToString(),
                Invoices = Invoices.ToList().Select(invoice => new PaymentInvoceData
                {
                    HeaderInvoiceId = invoice.Header_Invoice_ID,
                    BalanceToPay = invoice.Payment_Amount,
                }),
            };

            var response = await dataAccessService.PostPaymentReference(genereteRequest);
            IsGeneratePaymentReferece = false;
            StateHasChanged();
            if (response is null || !response.Operation_Succeeded || response.Result is null)
            {
                ShowMessage("Ocurrio un problema al tratar de generar la referencia de pago");
                return;
            }

            NavigateResumenCallback.Invoke(false);
            Reset.Invoke();

            var parameters = new DialogParameters
            {
                { "PaymentReference", response.Result },
                { "SendEmail", false }

            };

            // Open dialog and add if doesn't exist
            var options = new DialogOptions
            {
                CloseButton = true,
                MaxWidth = MaxWidth.Large,
                FullWidth = true,
                DisableBackdropClick = true
            };

            var dialog = DialogService.Show<ReferencePaymentOpenPayDialog>(localize.Get("msg_referencespei_title"), parameters, options);

            var result = await dialog.Result;

            if (result.Canceled)
            {
                return;
            }
        }

        private void ShowMessage(string message, Severity severityType = Severity.Error)
        {
            SnackbarServices.Configuration.PositionClass = Defaults.Classes.Position.BottomLeft;
            SnackbarServices.Add(LocalizeService.Get(message), severityType);
        }
        #endregion
    }
}
