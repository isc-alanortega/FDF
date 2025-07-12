using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using PCG_ENTITIES.PCG_FDF.CimaSimplex.Payments;
using PCG_ENTITIES.PCG_FDF.CimaSimplex.RazonSocial;
using PCG_ENTITIES.PCG_FDF.UtilityEntities;
using PCG_FDF.Components;
using PCG_FDF.Components.Dialogs;
using PCG_FDF.Data.ComponentDI.CimaSimlexUserManeuvers;

namespace PCG_FDF.Pages.Session
{
    public partial class UserManeuverBusinessName
    {
        #region INJECTS AND PROPERTIES
        [Inject] private CimaSimlexUserManeuverServices _cimaSimlexUserManeuverServices { get; set; }
        [Inject] IJSRuntime jsRuntime { get; set; }
        private GridListPager<CompanyInfoTriplet>? Pager_Reference { get; set; }
        private IEnumerable<CompanyInfoTriplet>? Business_Names { get; set; }
        private IEnumerable<CompanyInfoTriplet>? Selected_Business_Name { get; set; }
        private int Total_Business_Names { get; set; } = 0;
        private string Search_Query { get; set; } = string.Empty;
        private RazonSocial RazonSocialSelected { get; set; }
        #endregion

        #region METHODS
        protected async override Task OnInitializedAsync()
        {
            var response = await dataAccessService.GetCatCompanyNamesEmptyContainers(Search_Query, Total_Business_Names, 1);
            Total_Business_Names = response is not null && response.Total.HasValue ? response.Total.Value : 25;

            await UpdateSourceAsync();
            await base.OnInitializedAsync();
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _cimaSimlexUserManeuverServices._jsDownloadService = await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./scripts/download_helper.js");
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private async Task UpdateSourceAsync()
        {
            Business_Names = Enumerable.Empty<CompanyInfoTriplet>();

            var response = await dataAccessService.GetCatCompanyNamesEmptyContainers(Search_Query, Pager_Reference.ItemsPerPage, Pager_Reference.CurrentPage);
            if (response != null && response.Success)
            {
                Business_Names = response.Data;
                Total_Business_Names = response.Total ?? 0;
            }

            StateHasChanged();
        }

        private async Task OnValueChange(string value)
        {
            Search_Query = value;
            await UpdateSourceAsync();
        }

        private async void OnClickButtonRefresh() => await UpdateSourceAsync();

        private async void OnClickButtonAdd()
        {
            // Open dialog and add if doesn't exist
            var options = new DialogOptions
            {
                CloseButton = true,
                MaxWidth = MaxWidth.False,
                FullWidth = true,
                DisableBackdropClick = true
            };

            var dialog = DialogService.Show<ManeuverBusinessNameDialog>("", options);

            var result = await dialog.Result;

            if (result.Canceled || result.Data is null || (result.Data is bool success && !success))
            {
                return;
            }

            await UpdateSourceAsync();
        }

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
            //ShowResumen = true;
        }

        public void OnClickTotalPayment(PaymentInvoceResponse invoiceData)
        {
            //var invoicesList = SelectedInvoices.Invoices?.ToList() ?? new List<OpenPayInvoiceData>();

            //var existingInvoice = invoicesList.FirstOrDefault(i => i.Header_Invoice_ID == invoiceData.HeaderInvoiceId);

            //if (existingInvoice == null)
            //{
            //    // Add if doesn't exist
            //    invoicesList.Add(new OpenPayInvoiceData
            //    {
            //        Header_Invoice_ID = invoiceData.HeaderInvoiceId,
            //        Invoice = $"{invoiceData.InvoiceSerie} {invoiceData.InvoiceFolio}",
            //        Company_Name = invoiceData.ClientName,
            //        Date = invoiceData.StampDate,
            //        Payment_Amount = Convert.ToDecimal(invoiceData.InvoiceBalance)
            //    });
            //}
            //else
            //{
            //    // Delete if already exist
            //    invoicesList.Remove(existingInvoice);
            //}

            //// Reasigna la lista actualizada
            //SelectedInvoices.Invoices = invoicesList;

            //// Calculate the new Total
            //SelectedInvoices.Amount = invoicesList
            //    .Sum(i => i.Payment_Amount)
            //    .ToString("0.##");

            StateHasChanged();
        }

        public Color GetButtonColor(PaymentInvoceResponse invoiceData, bool isTotal)
        {
            return Color.Warning;
            //if (SelectedInvoices.Invoices == null)
            //    return Color.Primary;

            //var invoice = SelectedInvoices.Invoices.FirstOrDefault(invoice => invoice.Header_Invoice_ID == invoiceData.HeaderInvoiceId);
            
            //if (invoice is null)
            //    return Color.Primary;

            //bool match = invoice.Payment_Amount.ToString() == invoiceData.InvoiceBalance;

            //if ((isTotal && match) || (!isTotal && !match))
            //    return Color.Success;

            //return Color.Primary;
        }

        public bool DisabledPaymentButton(PaymentInvoceResponse invoiceData, bool isTotal)
        {
            return false;   
            //if (Convert.ToInt32(invoiceData.ReferenceStatus) == 2)
            //    return true;

            //if (SelectedInvoices.Invoices == null)
            //    return false;

            //var invoice = SelectedInvoices.Invoices.FirstOrDefault(invoice => invoice.Header_Invoice_ID == invoiceData.HeaderInvoiceId);
            //if (invoice is null)
            //    return false;

            //bool isMatch = invoice.Payment_Amount.ToString() == invoiceData.InvoiceBalance;

            //return isTotal ? !isMatch : isMatch;
        }

        private void NavigateOpenPayResumen(bool showResumen)
        {
            //ShowResumen = showResumen;
            StateHasChanged();
        }

        private async Task ResetComponent()
        {
            //SelectedInvoices = new OpenPayInvoices();
            await UpdateSourceAsync();
        }
        private async void OnViewRazon(int id)
        {
            try
            {
                var response = await dataAccessService.GetRazonSocial(id);
                if (response?.Operation_Succeeded == true && response.Result != null)
                {
                    var razonSocial = response.Result;

                    RazonSocialSelected = response.Result;
                    var parameters = new DialogParameters
            {
                { "RazonSocial", razonSocial },
                { "IsViewMode", true } // Activar modo vista
            };

                    var options = new DialogOptions
                    {
                        CloseButton = true,
                        MaxWidth = MaxWidth.Large,
                        FullWidth = true,
                        DisableBackdropClick = true
                    };

                    var dialog = DialogService.Show<ManeuverBusinessNameDialog>("Detalles de Razón Social", parameters, options);
                    var result = await dialog.Result;

                    if (!result.Canceled)
                    {
                        StateHasChanged();
                    }
                }
                else
                {
                    Snackbar.Add("No se pudo obtener la información de la razón social.", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add("Ocurrió un error al consultar la razón social.", Severity.Error);
            }
        }

        private async void OnEditRazon(int id)
        {
            try
            {
                var response = await dataAccessService.GetRazonSocial(id);
                if (response?.Operation_Succeeded == true && response.Result != null)
                {
                    RazonSocialSelected = response.Result;
                    int customerId = response.Result.IdCliente;
                    var parameters = new DialogParameters {
                  { "RazonSocial", RazonSocialSelected },
                  { "IsViewMode", false },
                  {"IsEditMode", true },
                  {"CustomerId", customerId }
              };
                    var options = new DialogOptions
                    {
                        CloseButton = true,
                        MaxWidth = MaxWidth.Large,
                        FullWidth = true,
                        DisableBackdropClick = true
                    };
                    DialogService.Show<ManeuverBusinessNameDialog>("Editar Razón Social", parameters, options);
                }
                else Snackbar.Add("No se pudo cargar la razón social.", Severity.Error);
            }
            catch
            {
                Snackbar.Add("Error al consultar la razón social.", Severity.Error);
            }
        }

        private async Task OnDesactivarRazon(int id)
        {
            try
            {
                var response = await dataAccessService.PutEliminarSocial(id);

                if (response?.Operation_Succeeded == true && response.Result != null && response.Result.Success)
                {
                    Snackbar.Add("Razón social eliminada correctamente.", Severity.Success);

                    // Opcional: refrescar la lista si tienes ese método
                    await UpdateSourceAsync();
                }
                else if (response?.Result?.Errors != null && response.Result.Errors.Any())
                {
                    // Mostrar el primer error (puedes mostrar más si gustas)
                    var primerError = response.Result.Errors.First();
                    string mensaje = !string.IsNullOrEmpty(primerError.Message)
                        ? primerError.Message
                        : "No se pudo eliminar la razón social.";

                    Snackbar.Add(mensaje, Severity.Warning);
                }
                else
                {
                    Snackbar.Add("No se pudo eliminar la razón social.", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add("Ocurrió un error al eliminar la razón social.", Severity.Error);
                Console.WriteLine($"Error al eliminar razón social: {ex.Message}");
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