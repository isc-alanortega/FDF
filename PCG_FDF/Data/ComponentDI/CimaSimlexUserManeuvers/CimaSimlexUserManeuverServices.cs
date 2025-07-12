using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using PCG_ENTITIES.Constants.CimaSimplex;
using PCG_ENTITIES.Enums;
using PCG_ENTITIES.PCG_FDF.CimaSimplex.Payments;
using PCG_ENTITIES.PCG_FDF.UtilityEntities;
using PCG_FDF.Data.ComponentDI.Booking;
using PCG_FDF.Data.DataAccess;
using PCG_FDF.Data.Localization;
using PCG_FDF.Data.Utils;

namespace PCG_FDF.Data.ComponentDI.CimaSimlexUserManeuvers
{
    public class CimaSimlexUserManeuverServices
    {
        #region INJECTIONS
        private readonly BookingDataCollection _bookingServices;
        private readonly PCG_FDF_DB _dataAccessServices;
        private readonly GlobalLocalizer _localizeService;
        private readonly ISnackbar _snackbarServices;
        private readonly NavigationManager _navigationManagerServices;

        public CimaSimlexUserManeuverServices(
            PCG_FDF_DB dataAccessService,
            BookingDataCollection bookingServices,
            GlobalLocalizer localizeService,
            ISnackbar snackbarServices,
            NavigationManager navigationManagerServices
        )
        {
            _dataAccessServices = dataAccessService;
            _bookingServices = bookingServices;
            _localizeService = localizeService;
            _snackbarServices = snackbarServices;
            _navigationManagerServices = navigationManagerServices;
        }
        #endregion

        public IJSObjectReference? _jsDownloadService;

        private bool IsDownloadZip { get; set; } = false;
        private bool IsEditingContract { get; set; } = false;
        private string MessageModal { get; set; } = "Cargando";
        public event Action OnChange;

        /// <summary>
        /// Dispara el evento de StateHasChanged y notifica al componente principal
        /// </summary>
        private void NotifyStateChanged() => OnChange?.Invoke();

        public bool ShowLoading() => IsDownloadZip;
        public string GetMessageModal() => MessageModal;

        #region CUSTOM COLOR
        public string GetColorsByStatus(string status) 
                => ManeuverStatusConstants.GetStatusIsSuspended(status) ? " background-color: yellow;" : "";

        #endregion
        
        #region DOWNLOAD ZIP
        public async void DownloadZip(string bookingFolio, string operationType)
        {
            try
            {
                if (IsDownloadZip) return;

                IsDownloadZip = true;
                MessageModal = "Descargando el pdf de las maniobras";

                if (!IsValidBookingFolio(bookingFolio))
                {
                    ShowMessage("snackbar_err_is_not_possible_download_zip");
                    IsDownloadZip = false;
                    NotifyStateChanged();
                    return;
                }

                var response = await _dataAccessServices.GetDownloadZipAsync(bookingFolio, operationType);
                if (!response.Operation_Succeeded || string.IsNullOrEmpty(response.Result))
                {
                    ShowMessage("snackbar_err_is_not_possible_download_zip");
                    IsDownloadZip = false;
                    NotifyStateChanged();
                    return;
                }
                string fileName = $"{bookingFolio}_{_localizeService.Get("lbl_maneuver")}";
                var fileStream = ConvertBase64ToStream.TryConvert(response.Result);
                if (fileStream is null)
                {
                    ShowMessage("snackbar_err_is_not_possible_download_zip");
                    IsDownloadZip = false;
                    NotifyStateChanged();
                    return;
                }

                using var streamRef = new DotNetStreamReference(stream: fileStream);
                await _jsDownloadService!.InvokeVoidAsync("downloadFileFromStream", $"{fileName}.pdf", streamRef);

                IsDownloadZip = false;
                NotifyStateChanged(); 

            }
            catch (Exception ex)
            {
                ShowMessage("snackbar_err_is_not_possible_download_zip");
                IsDownloadZip = false;
                NotifyStateChanged();
            }
        }

        public async Task DownloadZipInvoiceContainer(int? facturaencabezado)
        {
            try
            {
                if (facturaencabezado == null || facturaencabezado == 0)
                {
                    ShowMessage("snackbar_err_is_not_possible_download_zip");
                    return;
                }

                IsDownloadZip = true;
                MessageModal = "Descargando el ZIP";
                NotifyStateChanged(); 

                var response = await _dataAccessServices.GetDownloadZipInvoiceAsync(facturaencabezado);
                if (!response.Operation_Succeeded || string.IsNullOrEmpty(response.Result))
                {
                    ShowMessage("snackbar_err_is_not_possible_download_zip");
                    IsDownloadZip = false;
                    NotifyStateChanged();
                    return;
                }

                string fileName = $"{facturaencabezado}_{_localizeService.Get("label_invoice")}";
                var fileStream = ConvertBase64ToStream.TryConvert(response.Result);
                if (fileStream == null)
                {
                    ShowMessage("snackbar_err_is_not_possible_download_zip");
                    IsDownloadZip = false;
                    NotifyStateChanged();
                    return;
                }

                using var streamRef = new DotNetStreamReference(stream: fileStream);
                await _jsDownloadService!.InvokeVoidAsync("downloadFileFromStream", $"{fileName}.zip", streamRef);

                ShowMessage("snackbar_succ_download_invoice", Severity.Success);
                IsDownloadZip = false;
                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                ShowMessage("snackbar_err_is_not_possible_download_zip");
                IsDownloadZip = false;
                NotifyStateChanged();
            }
        }
        #endregion

        #region DOWNLOAD REFERENCE
        public async void DownloadInvoice(string invoiceId, string tipoOperacion)
        {
            try
            {
                if (IsDownloadZip) return;

                IsDownloadZip = true;
                MessageModal = "Descargando factura";

                var response = await _dataAccessServices.DownloadInvoiceAsync(invoiceId, tipoOperacion);
                if (!response.Operation_Succeeded || response.Result is null || string.IsNullOrEmpty(response.Result.Base64Zip))
                {
                    ShowMessage("snackbar_err_download_invoice");
                    IsDownloadZip = false;
                    NotifyStateChanged();
                    return;
                }

                string fileName = $"{invoiceId}_{_localizeService.Get("invoice")}";
                var fileStream = ConvertBase64ToStream.TryConvert(response.Result.Base64Zip);
                if (fileStream is null)
                {
                    ShowMessage("snackbar_err_download_invoice");
                    IsDownloadZip = false;
                    NotifyStateChanged();
                    return;
                }

                using var streamRef = new DotNetStreamReference(stream: fileStream);
                await _jsDownloadService!.InvokeVoidAsync("downloadFileFromStream", $"{fileName}.zip", streamRef);
                ShowMessage("snackbar_succ_download_invoice", Severity.Success);
                IsDownloadZip = false;
                NotifyStateChanged();
            }
            catch (Exception ex)
            {
                ShowMessage("No fue posible descargar favor de contactar a soporte ");
                IsDownloadZip = false;
                NotifyStateChanged();
                return;
            }
        }

        public async void DownloadReference(int invoiceId, string bookingFolio)
        {
            try
            {
                if(invoiceId == null || invoiceId==0)
                {
                    ShowMessage("El archivo de referencia no existe, comuníquese con soporte");
                    return;
                }
                if (IsDownloadZip) return;

                IsDownloadZip = true;
                MessageModal = "Descargando el documento de la referencia de pago";

                var response = await _dataAccessServices.PostDownloadPaymentVoucherByFileId(new PaymentDownloadByFileIdRequest()
                {
                    FileId = invoiceId,
                });

                if (response is null)
                {
                    ShowMessage("No fue posible descargar la referencia de pago");
                    IsDownloadZip = false;
                    NotifyStateChanged();
                    return;
                }

                string fileName = $"{bookingFolio}_{_localizeService.Get("payment_reference")}";
                using var streamRef = new DotNetStreamReference(stream: response);
                await _jsDownloadService!.InvokeVoidAsync("downloadFileFromStream", $"{fileName}.pdf", streamRef);

                IsDownloadZip = false;
                NotifyStateChanged();
            }
            catch(Exception ex)
            {
                ShowMessage("No fue posible descargar la referencia de pago");
                IsDownloadZip = false;
                NotifyStateChanged();
            }
        }
        #endregion

        #region EDIT BOOKING
        public async void EditBooking(string bookingFolio)
        {
            if(IsEditingContract) return;

            IsEditingContract = true;
            IsDownloadZip = true;

            if (!IsValidBookingFolio(bookingFolio))
            {
                IsDownloadZip = false;
                ShowMessage("snackbar_err_is_was_not_possible_edit_contract");
                NotifyStateChanged();
                return;
            }

            var response = await _dataAccessServices.GetBookingByFolioAsync(bookingFolio);
            if(response is null || !response.Operation_Succeeded || response.Result is null)
            {
                IsDownloadZip = false;
                ShowMessage("snackbar_err_is_was_not_possible_edit_contract");
                    NotifyStateChanged();
                return;
            }

            // reset booking 
            _bookingServices.ResetSelf();
            _bookingServices.SetStatus(EStatusContratacion.DATA_COMPLETED);
            _bookingServices.Contracting = false;
            _bookingServices.IsForceEdit = true;

            _navigationManagerServices.NavigateTo($"/book/{response.Result}");

            IsEditingContract = false;
            IsDownloadZip = false;
        }
        public async Task DownloadEIRContainer(string eir, string tipoOperacion)
        {
            if (string.IsNullOrWhiteSpace(eir))
            {
                ShowMessage("lbl_error_download_containereir");
                return;
            }
            try
            {
                var responseStream = await _dataAccessServices.DownloadEIRContainer(eir, tipoOperacion);
                using var streamRef = new DotNetStreamReference(stream: responseStream!);
                await _jsDownloadService!.InvokeVoidAsync("downloadFileFromStream", $"{eir}.pdf", streamRef);
                ShowMessage("lbl_succes_EIR", Severity.Success);
            }
            catch (Exception ex)
            {
                ShowMessage("No fue posible descargar favor de contactar a soporte");
            }
        }


        #endregion

        private void ShowMessage(string message, Severity severityType = Severity.Error)
        {
            _snackbarServices.Configuration.PositionClass = Defaults.Classes.Position.BottomLeft;
            _snackbarServices.Add(_localizeService.Get(message), severityType);
        }

        private bool IsValidBookingFolio(string bookingFolio)
            => !string.IsNullOrEmpty(bookingFolio) && !string.IsNullOrWhiteSpace(bookingFolio);
       
    }
}
