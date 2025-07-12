using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using PCG_ENTITIES.PCG_FDF.CimaSimplex.Payments;
using PCG_FDF.Components;
using PCG_FDF.Components.Dialogs;
using PCG_FDF.Data.ComponentDI.CimaSimlexUserManeuvers;
using PCG_FDF.Data.Entities;

namespace PCG_FDF.Pages.Session
{
    public partial class UserManeuverInvoices
    {
        #region INJECTS AND PROPERTIES
        [Inject] private CimaSimlexUserManeuverServices _cimaSimlexUserManeuverServices { get; set; }
        private GridListPager<PaymentInvoceResponse>? Pager_Reference { get; set; }
        private IEnumerable<PaymentInvoceResponse>? ManeuversInvoices { get; set; }
        private PaymentInvoceRequest Request { get; set; } = new PaymentInvoceRequest { Limit = 15, Page = 1, CurrencyId = 1 };
        private OpenPayInvoices SelectedInvoices { get; set; } = new OpenPayInvoices();
        private bool ShowResumen { get; set; } = false;
        private int TotalInvoices { get; set; } = 0;
        private bool Download_In_Progress { get; set; } = false;

        private IJSObjectReference? module_download;
        #endregion

        #region METHODS
        protected async override Task OnInitializedAsync()
        {
            var resultClientInfo = await dataAccessService.GetClientDocumentsDataEmptyContainers();
            if (resultClientInfo is not null && resultClientInfo.Operation_Succeeded && resultClientInfo.Result is not null)
            {
                Request.AgentId = resultClientInfo.Result!.IdAgente;
                Request.CustomId = resultClientInfo.Result!.IdAduana;
                //Request.AgentId = 1706;
                //Request.CustomId = 160;
            }

            await UpdateSourceAsync();
            await base.OnInitializedAsync();
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                module_download = await JS.InvokeAsync<IJSObjectReference>("import", "./scripts/download_helper.js");
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private async Task UpdateSourceAsync()
        {
            ManeuversInvoices = null;
            Request.Limit = Pager_Reference.ItemsPerPage;
            Request.Page = Pager_Reference.CurrentPage;

            var response = await dataAccessService.GetPaymentInvoce(Request);
            (List<PaymentInvoceResponse> data, int total) = (response is null || !response.Operation_Succeeded || response.Result is null)
                ? (new List<PaymentInvoceResponse>(), 0)
                : (response!.Result!.Invoices.ToList(), response!.Result!.Total);

            ManeuversInvoices = data;
            TotalInvoices = total;

            StateHasChanged();
        }

        private async Task OnValueChange(string value)
        {
            Request.Query = value;
            await UpdateSourceAsync();
        }

        private async void OnClickButtonRefresh() => await UpdateSourceAsync();


        public string GetColorsByStatus(string referenceStatus)
        {
            if (referenceStatus == "2")
            {
                return "background-color: #F1F6FF;";
            }
            return "";
        }

        public void OnClickOpenPay()
        {
            ShowResumen = true;
        }

        public string GetTotals(decimal amount)
        {
            return $"${amount:N2}";
        }

        public void OnClickTotalPayment(PaymentInvoceResponse invoiceData)
        {
            var invoicesList = SelectedInvoices.Invoices?.ToList() ?? new List<OpenPayInvoiceData>();

            var existingInvoice = invoicesList.FirstOrDefault(i => i.Header_Invoice_ID == invoiceData.HeaderInvoiceId);

            if (existingInvoice == null)
            {
                // Add if doesn't exist
                invoicesList.Add(new OpenPayInvoiceData
                {
                    Header_Invoice_ID = invoiceData.HeaderInvoiceId,
                    Invoice = $"{invoiceData.InvoiceSerie} {invoiceData.InvoiceFolio}",
                    Company_Name = invoiceData.ClientName,
                    Date = invoiceData.StampDate,
                    Payment_Amount = Convert.ToDecimal(invoiceData.InvoiceBalance)
                });
            }
            else
            {
                // Delete if already exist
                invoicesList.Remove(existingInvoice);
            }

            // Reasigna la lista actualizada
            SelectedInvoices.Invoices = invoicesList;

            // Calculate the new Total
            SelectedInvoices.Amount = invoicesList
                .Sum(i => i.Payment_Amount)
                .ToString("0.##");

            StateHasChanged();
        }

        public async void OnClickParcialPayment(PaymentInvoceResponse invoiceData)
        {
            var invoicesList = SelectedInvoices.Invoices?.ToList() ?? new List<OpenPayInvoiceData>();

            var existingInvoice = invoicesList.FirstOrDefault(i => i.Header_Invoice_ID == invoiceData.HeaderInvoiceId);

            if (existingInvoice == null)
            {
                var parameters = new DialogParameters
                {
                    { "InvoiceBalance", Convert.ToDecimal(invoiceData.InvoiceBalance) }
                };

                // Open dialog and add if doesn't exist
                var options = new DialogOptions
                {
                    CloseButton = true,
                    MaxWidth = MaxWidth.False,
                    FullWidth = false,
                    DisableBackdropClick = true
                };

                var dialog = DialogService.Show<ManeuverParcialPaymentDialog>("", parameters, options);
                
                var result = await dialog.Result;

                if (result.Canceled)
                {
                    return;
                }

                decimal amountPayment = (decimal)result.Data;

                invoicesList.Add(new OpenPayInvoiceData
                {
                    Header_Invoice_ID = invoiceData.HeaderInvoiceId,
                    Invoice = $"{invoiceData.InvoiceSerie} {invoiceData.InvoiceFolio}",
                    Company_Name = invoiceData.ClientName,
                    Date = invoiceData.StampDate,
                    Payment_Amount = (decimal)result.Data
                });

                Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomLeft;
                Snackbar.Add($"{localize.Get("msg_paymentamount")}{amountPayment} {localize.Get("msg_ofinvoice")} {invoiceData.InvoiceSerie} {invoiceData.InvoiceFolio}.", Severity.Success);
            }
            else
            {
                // Delete if already exist
                invoicesList.Remove(existingInvoice);
            }

            // Reasigna la lista actualizada
            SelectedInvoices.Invoices = invoicesList;

            // Calculate the new Total
            SelectedInvoices.Amount = invoicesList
                .Sum(i => i.Payment_Amount)
                .ToString("0.##");

            StateHasChanged();
        }

        public async void OnClickDeleteReference(PaymentInvoceResponse invoiceData)
        {
            var parameters = new DialogParameters
            {
                { "InvoiceData", invoiceData }
            };

            var options = new DialogOptions
            {
                CloseButton = true,
                MaxWidth = MaxWidth.Medium,
                FullWidth = false,
                DisableBackdropClick = true
            };

            var dialog = DialogService.Show<DeleteReferenceManeuversDialog>(localize.Get("btn_deletereference"), parameters, options);

            var result = await dialog.Result;

            if (result.Canceled)
            {
                return;
            }

            await UpdateSourceAsync();
        }

        private async Task DownloadZip(PaymentInvoceResponse invoice)
        {
            try
            {
                if (!Download_In_Progress)
                {
                    Download_In_Progress = true;

                    var FILESTREAM = await dataAccessService.PostDownloadEmptyInvoiceZip(invoice.HeaderInvoiceId);
                    if (FILESTREAM is not null && module_download is not null)
                    {
                        var fileName = $"Archivos_Factura_{invoice.InvoiceSerie}_{invoice.InvoiceFolio}.zip";
                        using var streamRef = new DotNetStreamReference(stream: FILESTREAM);
                        await module_download.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);

                        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomLeft;
                        Snackbar.Add(localize.Get("msg_zip_successfully_downloaded"), Severity.Success);

                        return;
                    }

                    Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomLeft;
                    Snackbar.Add(localize.Get("error_zip_downloaded"), Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomLeft;
                Snackbar.Add(localize.Get("error_zip_downloaded"), Severity.Error);
            }
            finally
            {
                Download_In_Progress = false;
            }
        }
        private async Task DownloadZipInvoice(PaymentInvoceResponse invoice)
        {
            try
            {
                if (!Download_In_Progress)
                {
                    Download_In_Progress = true;

                    var FILESTREAM = await dataAccessService.PostDownloadInvoiceZip(invoice.HeaderInvoiceId);
                    if (FILESTREAM is not null && module_download is not null)
                    {
                        var fileName = $"Archivos_Factura_{invoice.InvoiceSerie}_{invoice.InvoiceFolio}.zip";
                        using var streamRef = new DotNetStreamReference(stream: FILESTREAM);
                        await module_download.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);

                        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomLeft;
                        Snackbar.Add(localize.Get("msg_zip_successfully_downloaded"), Severity.Success);

                        return;
                    }

                    Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomLeft;
                    Snackbar.Add(localize.Get("error_zip_downloaded"), Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomLeft;
                Snackbar.Add(localize.Get("error_zip_downloaded"), Severity.Error);
            }
            finally
            {
                Download_In_Progress = false;
            }
        }

        public Color GetButtonColor(PaymentInvoceResponse invoiceData, bool isTotal)
        {
            if (SelectedInvoices.Invoices == null)
                return Color.Primary;

            var invoice = SelectedInvoices.Invoices.FirstOrDefault(invoice => invoice.Header_Invoice_ID == invoiceData.HeaderInvoiceId);
            
            if (invoice is null)
                return Color.Primary;

            bool match = invoice.Payment_Amount.ToString() == invoiceData.InvoiceBalance;

            if ((isTotal && match) || (!isTotal && !match))
                return Color.Success;

            return Color.Primary;
        }

        public bool DisabledPaymentButton(PaymentInvoceResponse invoiceData, bool isTotal)
        {
            if (Convert.ToInt32(invoiceData.ReferenceStatus) == 2)
                return true;

            if (SelectedInvoices.Invoices == null)
                return false;

            var invoice = SelectedInvoices.Invoices.FirstOrDefault(invoice => invoice.Header_Invoice_ID == invoiceData.HeaderInvoiceId);
            if (invoice is null)
                return false;

            bool isMatch = invoice.Payment_Amount.ToString() == invoiceData.InvoiceBalance;

            return isTotal ? !isMatch : isMatch;
        }

        private void NavigateOpenPayResumen(bool showResumen)
        {
            ShowResumen = showResumen;
            StateHasChanged();
        }

        private async Task ResetComponent()
        {
            SelectedInvoices = new OpenPayInvoices();
            await UpdateSourceAsync();
        }

        /*
         * Eliminar los modulos JS cargados al terminar de usar el componente actual
         */
        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (module_download is not null)
            {
                await module_download.DisposeAsync();
            }
        }
        #endregion

        #region STYLES
        string invoices_wrapper = "padding-top: 10px; display: flex; justify-content: center";
        string paper_grey = "border-radius: 15px; margin-top: 40px; margin-bottom: 40px; background-image: linear-gradient(to bottom, var(--color-global-fullwhite), var(--color-global-theme-grey)); padding: 20px; display: flex; flex-direction: column; align-items: center; justify-content: flex-start";
        string pager_header_wrapper = "padding: 8px; width: 100%";
        string pager_header = "display: grid; grid-template-columns: repeat(auto-fit, minmax(100px, 1fr)); align-items: flex-start; justify-content: space-between; gap: 16px; padding: 8px 12px";
        string inner_row = "width: 100%; display: grid; grid-template-columns: repeat(auto-fit, minmax(100px, 1fr)); align-items: flex-start; justify-content: space-between; gap: 16px; padding: 8px 12px";
        string usermaneuverinvoices_griditemdisplay = "width: 100%; max-width: 450px; min-width: 250px; height: auto; margin: 10px auto; padding: 20px; box-sizing: border-box; display: flex; flex-direction: column";
        #endregion
    }
}