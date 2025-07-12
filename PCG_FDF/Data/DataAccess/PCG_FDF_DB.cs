using Newtonsoft.Json;
using System.Net;
using PCG_ENTITIES.PCG;
using System.Net.Http.Headers;
using PCG_ENTITIES.PCG_FDF.UtilityEntities;
using PCG_ENTITIES.Enums;
using System.Text;
using PCG_ENTITIES.PCG.Session;
using PCG_FDF.Data.ComponentDI.AuthManagement;
using Microsoft.AspNetCore.Components;
using PCG_ENTITIES.Resultsets;
using PCG_ENTITIES.PCG_FDF.BookingEntities;
using Microsoft.AspNetCore.Components.Forms;
using PCG_ENTITIES.Requests;
using PCG_ENTITIES.PCG_FDF.TrackingEntities;
using PCG_ENTITIES.PCG_FDF.PortCapital;
using PCG_ENTITIES.PCG_FDF.CimaSimplex;
using PCG_ENTITIES.PCG_FDF.CimaSimplex.Payments;
using Syncfusion.Blazor.Data;
using PCG_ENTITIES.PCG_FDF.CimaSimplex.Customers;
using Azure;
using PCG_FDF.Data.Entities;
using System.Security.Cryptography.X509Certificates;
using PCG_ENTITIES.PCG_FDF.CimaSimplex.Containers;
using PCG_ENTITIES.PCG_FDF.CimaSimplex.RazonSocial;
using System.Net.Http.Json;
using PCG_ENTITIES.PCG_FDF.CimaSimplex.Countries;

namespace PCG_FDF.Data.DataAccess
{
    public class PCG_FDF_DB
    {
        private bool initialized = false;
        private readonly HttpClient _HttpClient;
        private AuthService _AuthService;
        private readonly NavigationManager _navigator;

        public PCG_FDF_DB(HttpClient http, NavigationManager navigator)
        {
            _HttpClient = http;
            _navigator = navigator;
            _HttpClient.DefaultRequestHeaders.Add("Accept", "text/plain");
            _HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public void Initialize(IAuthService authService)
        {
            if (!initialized)
            {
                _AuthService = (AuthService)authService;
                initialized = true;
            }
        }

        #region BETA

        public async Task<T?> SendUnauthTAsync<T>(string Endpoint, HttpMethod Method, IDictionary<string, string>? Headers, string? Request_Content)
        {
            var request = new HttpRequestMessage(Method, new Uri(_HttpClient.BaseAddress!, Endpoint));

            if (Headers is not null && Headers.Any())
            {
                foreach (var header in Headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            if (Request_Content is not null)
            {
                request.Content = new StringContent(Request_Content, Encoding.UTF8, "application/json");
            }

            var response = await _HttpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return default;
            }

            var result = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());

            return result;
        }

        public async Task<T?> SendAuthTAsync<T>(string Endpoint, HttpMethod Method, IDictionary<string, string?>? Headers = null, string? Request_Content = null, MultipartFormDataContent? Form = null)
        {
            var request = new HttpRequestMessage(Method, new Uri(_HttpClient.BaseAddress!, Endpoint));

            if (Headers is not null && Headers.Any())
            {
                foreach (var header in Headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            if (Request_Content is not null)
            {
                request.Content = new StringContent(Request_Content, Encoding.UTF8, "application/json");
            }
            else if (Form is not null)
            {
                request.Content = Form;
            }

            var response = await _HttpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refresh_success = await _AuthService.Refresh();
                if (refresh_success)
                {
                    return await SendAuthTAsync<T>(Endpoint, Method, Headers, Request_Content);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return default;
                }
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return default;
            }

            var result = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());

            return result;
        }

        public async Task<Stream?> DownloadAuthAsync(string Endpoint, HttpMethod Method, IDictionary<string, string?>? Headers = null, string? Request_Content = null, MultipartFormDataContent? Form = null)
        {
            var request = new HttpRequestMessage(Method, new Uri(_HttpClient.BaseAddress!, Endpoint));

            if (Headers is not null && Headers.Any())
            {
                foreach (var header in Headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            if (Request_Content is not null)
            {
                request.Content = new StringContent(Request_Content, Encoding.UTF8, "application/json");
            }
            else if (Form is not null)
            {
                request.Content = Form;
            }

            var response = await _HttpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refresh_success = await _AuthService.Refresh();
                if (refresh_success)
                {
                    return await DownloadAuthAsync(Endpoint, Method, Headers, Request_Content);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return default;
                }
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return default;
            }

            var result = await response.Content.ReadAsStreamAsync();

            return result;
        }
        #endregion

        #region UNAUTHORIZED

        public async Task<IEnumerable<CategoriasEntidad>> GetCategoriasPrincipales(string lenguaje)
        {
            var urlLog = "/PCGServicios/GetPCGServiciosCategoriasPrincipales";

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Headers.Add("language", lenguaje);

            var respuesta = await _HttpClient.SendAsync(request);

            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var resultado = JsonConvert.DeserializeObject<IEnumerable<CategoriasEntidad>>(await respuesta.Content.ReadAsStringAsync());
            return resultado.OrderBy(res => res.Secuencia);
        }

        public async Task<IEnumerable<CategoriasEntidad>> GetCategoriasPrincipalesFiltradas(string lenguaje, string CategoryIDs)
        {
            var urlLog = "/PCGServicios/GetPCGServiciosCategoriasPrincipalesFiltradas";

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Headers.Add("language", lenguaje);
            request.Headers.Add("CategoryIDs", CategoryIDs);

            var respuesta = await _HttpClient.SendAsync(request);

            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var resultado = JsonConvert.DeserializeObject<IEnumerable<CategoriasEntidad>>(await respuesta.Content.ReadAsStringAsync());
            return resultado.OrderBy(res => res.Secuencia);
        }

        public async Task<CategoriasFiltradas> GetCategoriasFiltradas(int UnitID)
        {
            var urlLog = "/PCGServicios/GetPCGServiciosCategoriasFiltradas";
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Headers.Add("UnitID", Convert.ToString(UnitID));

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<CategoriasFiltradas>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<IEnumerable<CategoriasEntidad>> GetCategorias(string lenguaje, int padre)
        {
            var urlLog = "/PCGServicios/GetPCGServiciosCategorias";
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Headers.Add("language", lenguaje);
            request.Headers.Add("parent", Convert.ToString(padre));

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<IEnumerable<CategoriasEntidad>>(await respuesta.Content.ReadAsStringAsync());
            return resultado.OrderBy(res => res.Secuencia);
        }

        public async Task<bool?> GetServiciosExist(string lenguaje, int? UnitID)
        {
            var urlLog = "/PCGServicios/GetPCGServiciosCheck";
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Headers.Add("language", lenguaje);
            request.Headers.Add("UnitID", Convert.ToString(UnitID));

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<bool>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<IEnumerable<ServiciosCompletosEntidad>> GetServicios(string lenguaje, int padre, int? UnitID)
        {
            var urlLog = "/PCGServicios/GetPCGServicios";
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Headers.Add("language", lenguaje);
            request.Headers.Add("parent", Convert.ToString(padre));
            request.Headers.Add("UnitID", Convert.ToString(UnitID));

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<IEnumerable<ServiciosCompletosEntidad>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<MiddlewareQuotation> GetQuotationObject(Guid GUID)
        {
            var urlLog = "/PCG_FDFCotizaciones/GetPCG_FDFObjetoCotizacion";

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Headers.Add("QuotationUUID", Convert.ToString(GUID.ToString()));

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<MiddlewareQuotation>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<QuotationReInitializerResult> PostQuotationObject(Guid GUID)
        {
            var urlLog = "/PCG_FDFCotizaciones/PostObjetoCotizacion";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(GUID));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<QuotationReInitializerResult>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<MiddlewareShoppingCart> GetShoppingCartObject(string language, IEnumerable<int> serviceIDs, IEnumerable<int> packageIDs)
        {
            var urlLog = "/PCG_FDFCotizaciones/PostPCG_FDFObjetoCarrito";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(new ShoppingCartIDsWrapper() { Language = language, PackageIDs = packageIDs, ServiceIDs = serviceIDs }));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<MiddlewareShoppingCart>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<APIResult<PaquetesResult>> GetPaquetes(string lenguaje, int? UnitID, int? ClientID, int PageNumber, int ItemsPerPage)
        {
            var urlLog = "/PCGPaquetes/GetPCGPaquetes";
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Headers.Add("language", lenguaje);
            request.Headers.Add("UnitID", Convert.ToString(UnitID));
            request.Headers.Add("ClientID", Convert.ToString(ClientID));
            request.Headers.Add("PageNumber", Convert.ToString(PageNumber));
            request.Headers.Add("ItemsPerPage", Convert.ToString(ItemsPerPage));

            var respuesta = await _HttpClient.SendAsync(request);

            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<APIResult<PaquetesResult>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<IEnumerable<PaquetesCompletosEntidad>> GetBuscarPaquetes(string lenguaje, string busqueda, int? UnitID)
        {
            var urlLog = "/PCGPaquetes/GetPCGBusquedaPaquetes";
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Headers.Add("language", lenguaje);
            request.Headers.Add("search", busqueda);
            request.Headers.Add("UnitID", Convert.ToString(UnitID));

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<IEnumerable<PaquetesCompletosEntidad>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<IEnumerable<ServiciosTraceEntidad>> GetBuscarServicios(string lenguaje, string busqueda, int? UnitID)
        {
            var urlLog = "/PCGServicios/GetPCGBusquedaServicios";
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Headers.Add("language", lenguaje);
            request.Headers.Add("search", busqueda);
            request.Headers.Add("UnitID", Convert.ToString(UnitID));

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<IEnumerable<ServiciosTraceEntidad>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<PaquetesCompletosEntidad> GetPaqueteMatch(string lenguaje, string serviceIDs, int? UnitID)
        {
            var urlLog = "/PCGPaquetes/GetPCGPaquetesMatch";
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Headers.Add("language", lenguaje);
            request.Headers.Add("serviceIDs", serviceIDs);
            request.Headers.Add("UnitID", Convert.ToString(UnitID));

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<PaquetesCompletosEntidad>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<string> GetParametro(int parameterID)
        {
            var urlLog = "/PCGParametros/GetPCGParametro";
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Headers.Add("parameterID", Convert.ToString(parameterID));

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<string>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<HttpResponseMessage> PostUserLogin(LoginModel user_data)
        {
            var urlLog = "/PCG_Login/PostPCGFDFLogin";

            var loginAsJson = JsonConvert.SerializeObject(user_data);
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(loginAsJson, Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            return respuesta;
        }

        public async Task<HttpResponseMessage> PostRefreshLogin(RefreshModel refresh_data)
        {
            var urlLog = "/PCG_Login/PostPCGFDFRefresh";

            var loginAsJson = JsonConvert.SerializeObject(refresh_data);
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(loginAsJson, Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            return respuesta;
        }

        public async Task PostRevokeLogin(RefreshModel login_data)
        {
            var urlLog = "/PCG_Login/PostPCGLogout";

            var loginAsJson = JsonConvert.SerializeObject(login_data);
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(loginAsJson, Encoding.UTF8, "application/json");

            await _HttpClient.SendAsync(request);
        }

        public async Task<Stream> PostDownloadPDF(Guid GUID, string language, EPages Page)
        {
            var urlLog = "/PCG_FDFCotizaciones/GetPCG_FDFDescargarCotizacionPDF";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Headers.Add("QuotationUUID", Convert.ToString(GUID.ToString()));
            request.Headers.Add("language", language);
            request.Headers.Add("Page", Page.ToString());

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = await respuesta.Content.ReadAsStreamAsync();
            return resultado;
        }

        public async Task<Stream> PostDownloadAgreementPDF(Guid GUID, string language)
        {
            var urlLog = "/PCGAcuerdos/PostPCGDescargarAcuerdoPDF";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Headers.Add("Agreement_UUID", Convert.ToString(GUID.ToString()));
            request.Headers.Add("language", language);

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = await respuesta.Content.ReadAsStreamAsync();
            return resultado;
        }

        public async Task<ThemeData> GetThemeData(string SiteUri)
        {
            var urlLog = "/PCG_FDFTheme/GetPCG_FDFSiteTheme";

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Headers.Add("Site", SiteUri);

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<ThemeData>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<IDictionary<string, bool>> GetSiteConfig(string SiteUri)
        {
            var urlLog = "/PCG_FDFTheme/GetPCG_FDFSiteConfig";

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Headers.Add("Site", SiteUri);

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<IDictionary<string, bool>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<(bool Success, AgreementSigningResult? Result)> SignAgreement(AgreementSigning data)
        {
            var urlLog = "/PCGAcuerdos/PostFirmarAcuerdo";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(data));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return (false, null);
            }
            var resultado = JsonConvert.DeserializeObject<AgreementSigningResult>(await respuesta.Content.ReadAsStringAsync());
            return (true, resultado);
        }

        #endregion

        #region AUTHORIZED

        public async Task<IEnumerable<ServiciosElementosCompletosEntidad>> GetElementosServicio(string lenguaje, int serviceID)
        {
            var urlLog = "/PCGElementos/GetPCGElementos";
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Headers.Add("language", lenguaje);
            request.Headers.Add("serviceID", Convert.ToString(serviceID));

            var respuesta = await _HttpClient.SendAsync(request);

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();

                if (refreshSuccess)
                {
                    return await GetElementosServicio(lenguaje, serviceID);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var resultado = JsonConvert.DeserializeObject<IEnumerable<ServiciosElementosCompletosEntidad>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<IDictionary<int, Dictionary<int, HashSet<Guid>>>> GetExclusiones(IEnumerable<int> serviceIDs)
        {
            var urlLog = "/PCGElementos/GetPCGElementosExclusiones";
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Headers.Add("serviceIDs", string.Join(",", serviceIDs));

            var respuesta = await _HttpClient.SendAsync(request);

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();

                if (refreshSuccess)
                {
                    return await GetExclusiones(serviceIDs);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var resultado = JsonConvert.DeserializeObject<IDictionary<int, Dictionary<int, HashSet<Guid>>>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<string> GetBuscarCatalogosDinamicos(string busqueda, int CatalogID)
        {
            var urlLog = "/PCGServicios/GetPCGBusquedaCatalogosDinamicos";
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Headers.Add("search", busqueda);
            request.Headers.Add("CatalogID", Convert.ToString(CatalogID));

            var respuesta = await _HttpClient.SendAsync(request);

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();

                if (refreshSuccess)
                {
                    return await GetBuscarCatalogosDinamicos(busqueda, CatalogID);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var resultado = await respuesta.Content.ReadAsStringAsync();
            return resultado;
        }

        public async Task<IEnumerable<ServicioCamposTarifariosEntidad>> GetCamposTarifariosServicio(int serviceID)
        {
            var urlLog = "/PCGTarifarios/GetPCGCamposServicio";
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Headers.Add("serviceID", Convert.ToString(serviceID));

            var respuesta = await _HttpClient.SendAsync(request);

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();

                if (refreshSuccess)
                {
                    return await GetCamposTarifariosServicio(serviceID);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var resultado = JsonConvert.DeserializeObject<IEnumerable<ServicioCamposTarifariosEntidad>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<TariffCalculationResult> GetCalculatedTariff(ClientQuotationData data)
        {
            var urlLog = "/PCGTarifarios/GetPCGCalcularTarifa";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(data));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var respuesta = await _HttpClient.SendAsync(request);

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();

                if (refreshSuccess)
                {
                    return await GetCalculatedTariff(data);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var resultado = JsonConvert.DeserializeObject<TariffCalculationResult>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<ResultadoCotizacionEntidad?> PostSerializeQuotation(MiddlewareQuotation data)
        {
            try
            {
                var urlLog = "/PCG_FDFCotizaciones/PostSerializarTarifa";

                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
                request.Content = new StringContent(JsonConvert.SerializeObject(data));
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var respuesta = await _HttpClient.SendAsync(request);
                if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var refreshSuccess = await _AuthService.Refresh();
                    if (refreshSuccess)
                    {
                        return await PostSerializeQuotation(data);
                    }
                    else
                    {
                        _navigator.NavigateTo("/login", true, true);
                        return null;
                    }
                }
                else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
                {
                    return null;
                }
                var resultado = JsonConvert.DeserializeObject<ResultadoCotizacionEntidad>(await respuesta.Content.ReadAsStringAsync());
                return resultado;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<StringResult?> PostSaveQuotation(SaveQuotationRequest data)
        {
            try
            {
                var urlLog = "/PCG_FDFCotizaciones/PostGuardarCotizacion";

                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
                request.Content = new StringContent(JsonConvert.SerializeObject(data));
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var respuesta = await _HttpClient.SendAsync(request);
                if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var refreshSuccess = await _AuthService.Refresh();
                    if (refreshSuccess)
                    {
                        return await PostSaveQuotation(data);
                    }
                    else
                    {
                        _navigator.NavigateTo("/login", true, true);
                        return null;
                    }
                }
                else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
                {
                    return null;
                }
                var resultado = JsonConvert.DeserializeObject<StringResult>(await respuesta.Content.ReadAsStringAsync());
                return resultado;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<int?> GetCoins(int UserID)
        {
            var urlLog = "/PCGUsuarios/PCGUsuarioGetCoins";

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Headers.Add("UserID", Convert.ToString(UserID));

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await GetCoins(UserID);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<int>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<APIResult<UserQuotes>> GetUserQuotation(UserQuotationsRequest data)
        {
            var urlLog = "/PCG_FDFCotizaciones/PostPCG_FDFCotizacionesUsuario";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(data));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await GetUserQuotation(data);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<APIResult<UserQuotes>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<BookingFormResult> PostGetBookingForm(RequestBookingForm RequestData)
        {
            var urlLog = "/PCG_FDFContrataciones/PostObtenerFormularioContratacion";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(RequestData), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await PostGetBookingForm(RequestData);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<BookingFormResult>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        // Key = User_ID, Value = Client_ID
        public async Task<APIResult<LocalitiesData>> PostGetLocalities(KeyValueInt Data)
        {
            var urlLog = "/PCG_Login/PostPCGFDFLocalities";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(Data), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await PostGetLocalities(Data);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<APIResult<LocalitiesData>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<APIResult<IDictionary<int, ClienteVinculadoEntidad>>> PostGetSubclients(int UserID)
        {
            var urlLog = "/PCG_Login/PostPCGFDFSubclients";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(UserID), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await PostGetSubclients(UserID);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<APIResult<IDictionary<int, ClienteVinculadoEntidad>>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<QuotationFormResult?> PostGetQuotationForm(RequestQuotationForm RequestData)
        {
            var urlLog = "/PCG_FDFCotizaciones/PostObtenerFormularioCotizacion";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(RequestData), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await PostGetQuotationForm(RequestData);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<QuotationFormResult>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<BooleanResult> PostSaveBookingForm(BookingMiddleware booking_data)
        {
            var urlLog = "/PCG_FDFContrataciones/PostGuardarBooking";

            var book_as_json = JsonConvert.SerializeObject(booking_data);

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(book_as_json, Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await PostSaveBookingForm(booking_data);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return JsonConvert.DeserializeObject<BooleanResult>(await respuesta.Content.ReadAsStringAsync());
            }
            var resultado = JsonConvert.DeserializeObject<BooleanResult>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<APIResult<UserContracts>?> GetUserBookings(UserBookingsRequest query)
        {
            var urlLog = "/PCG_FDFContrataciones/GetObtenerBookings";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(query), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await GetUserBookings(query);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<APIResult<UserContracts>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<BookingReInitializerResult?> PostBookingData(BookingRequestModel request_data)
        {
            var urlLog = "/PCG_FDFContrataciones/PostObtenerDatosBooking";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(request_data), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await PostBookingData(request_data);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<BookingReInitializerResult>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<DynamicQueryResultUUID?> PostLeerQuery(DynamicQueryRequestQuotation request_data)
        {
            var urlLog = "/PCG_FDFCotizaciones/PostObtenerQuery";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(request_data), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await PostLeerQuery(request_data);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<DynamicQueryResultUUID>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<DynamicQueryResult?> PostLeerQuery(DynamicQueryRequestBooking request_data)
        {
            var urlLog = "/PCG_FDFContrataciones/PostObtenerQuery";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(request_data), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await PostLeerQuery(request_data);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<DynamicQueryResult>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<DynamicQueryResultSAT?> PostLeerQuerySAT(DynamicQueryRequestBooking request_data)
        {
            var urlLog = "/PCG_FDFContrataciones/PostObtenerQuerySAT";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(request_data), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await PostLeerQuerySAT(request_data);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<DynamicQueryResultSAT>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<APIResult<IEnumerable<SATMerchQuadruplet>>?> PostObtenerQuerySATQuad(DynamicQueryRequestBooking request_data)
        {
            var urlLog = "/PCG_FDFContrataciones/PostObtenerQuerySATQuad";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(request_data), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await PostObtenerQuerySATQuad(request_data);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<APIResult<IEnumerable<SATMerchQuadruplet>>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<IntegerResult> PostUploadBookingFile(IBrowserFile File, int Document_Subtype_ID, string Booking_ID, int User_ID, int Client_ID, int? Uploaded_Document_ID)
        {
            var urlLog = "/PCG_FDFContrataciones/PostBookingDocument";

            var formData = new MultipartFormDataContent
            {
                // Add the file to the form data
                // 50Mb Limit
                { new StreamContent(File.OpenReadStream(maxAllowedSize: 51200 * 1024)), "File", File.Name },

                // Add additional data to the form data
                { new StringContent(Document_Subtype_ID.ToString()), "Document_Subtype_ID" },

                // Add additional data to the form data
                { new StringContent(User_ID.ToString()), "User_ID" },

                // Add additional data to the form data
                { new StringContent(Client_ID.ToString()), "Client_ID" },

                // Add additional data to the form data
                { new StringContent(Booking_ID), "Booking_ID" },

                // Add additional data to the form data
                { new StringContent(Uploaded_Document_ID.HasValue ? Uploaded_Document_ID.Value.ToString() : string.Empty), "Uploaded_Document_ID" }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = formData;

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await PostUploadBookingFile(File, Document_Subtype_ID, Booking_ID, User_ID, Client_ID, Uploaded_Document_ID);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var resultado = JsonConvert.DeserializeObject<IntegerResult>(await respuesta.Content.ReadAsStringAsync());
            return resultado;

        }

        public async Task<Stream> PostDownloadBookingDocument(int Document_ID)
        {
            var urlLog = "/PCG_FDFContrataciones/PostPCG_FDFDescargarDocumentoBooking";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Headers.Add("Document_ID", Convert.ToString(Document_ID));

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await PostDownloadBookingDocument(Document_ID);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = await respuesta.Content.ReadAsStreamAsync();
            return resultado;
        }

        public async Task<Stream> PostDownloadInvoiceDocument(InvoiceInfo invoice, bool downloadPDF)
        {
            var urlLog = downloadPDF
                ? "/PCG_FDF_Pagos/PostDownloadInvoicePDF"
                : "/PCG_FDF_Pagos/PostDownloadInvoiceXML";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(invoice), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await PostDownloadInvoiceDocument(invoice, downloadPDF);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = await respuesta.Content.ReadAsStreamAsync();
            return resultado;
        }

        public async Task<Stream> PostDownloadBookingPDF(Guid Booking_UUID)
        {
            var urlLog = "/PCG_FDFContrataciones/PostPCG_FDFDescargarPDFBooking";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Headers.Add("Booking_UUID", Booking_UUID.ToString());

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await PostDownloadBookingPDF(Booking_UUID);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = await respuesta.Content.ReadAsStreamAsync();
            return resultado;
        }

        public async Task<APIResult<bool>> PostTryCloneBooking(CloneBookingRequest data)
        {
            var urlLog = "/PCG_FDFContrataciones/PostClonarBooking";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await PostTryCloneBooking(data);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var resultado = JsonConvert.DeserializeObject<APIResult<bool>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<APIResult<bool>> PostSaveMetadata(DocumentMetadata data)
        {
            var urlLog = "/PCG_FDFContrataciones/PostPCG_FDFGuardarMetadatos";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await PostSaveMetadata(data);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var resultado = JsonConvert.DeserializeObject<APIResult<bool>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<APIResult<FavoriteResult>> PostUpdateFavorite(ActualizarPaqueteFavorito data)
        {
            var urlLog = "/PCGPaquetes/PostPCG_FDFFavoritePackage";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await PostUpdateFavorite(data);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var resultado = JsonConvert.DeserializeObject<APIResult<FavoriteResult>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<APIResult<IEnumerable<KeyValueUUID>>> PostGetActiveBookings(UserActiveBookings data)
        {
            var urlLog = "/PCG_FDFContrataciones/PostPCG_FDFBookingsActivos";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await PostGetActiveBookings(data);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var resultado = JsonConvert.DeserializeObject<APIResult<IEnumerable<KeyValueUUID>>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<APIResult<TrackingInitializer?>> PostGetBookingTracking(RequestTrackingForm data)
        {
            var urlLog = "/PCG_FDFTracking/PostGetBookingTracking";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await PostGetBookingTracking(data);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var resultado = JsonConvert.DeserializeObject<APIResult<TrackingInitializer?>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<APIResult<ExcelResult<T>?>?> PostUploadExcel<T>(IBrowserFile File, EDataType DataType, int Amount, decimal? Unit_ID = null, ShippingLineInfoTriplet? ShippingCompany = null)
        {
            var urlLog = "/PCG_FDFContrataciones/PostLoadExcelData";

            var formData = new MultipartFormDataContent
            {
                // Add the file to the form data
                // 50Mb Limit
                { new StreamContent(File.OpenReadStream(maxAllowedSize: 51200 * 1024)), "File", File.Name },

                // Add additional data to the form data
                { new StringContent(((int)DataType).ToString()), "DataType" },

                // Add additional data to the form data
                { new StringContent(Amount.ToString()), "Amount" }
            };

            if (Unit_ID.HasValue)
            {
                formData.Add(new StringContent(Unit_ID.Value.ToString()), "Unit_ID");
            }

            if (ShippingCompany != null)
            {
                formData.Add(new StringContent(JsonConvert.SerializeObject(ShippingCompany), Encoding.UTF8, "application/json"), "ShippingCompany");
            }

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = formData;

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await PostUploadExcel<T>(File, DataType, Amount);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var resultado = JsonConvert.DeserializeObject<APIResult<ExcelResult<T>?>?>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<Stream?> PostDownloadExcelForm(EDataType DataType)
        {
            var urlLog = "/PCG_FDFContrataciones/PostPCG_FDFDescargarPlantillaExcel";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Headers.Add("DataType", ((int)DataType).ToString());

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await PostDownloadExcelForm(DataType);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = await respuesta.Content.ReadAsStreamAsync();
            return resultado;
        }

        public async Task<APIResult<UserReports>?> GetUserHandling(UserReportsRequest query)
        {
            var urlLog = "/PCG_FDFContrataciones/GetObtenerManiobras";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(query), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await GetUserHandling(query);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<APIResult<UserReports>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }
        #endregion

        #region PORT CAPITAL REQUEST 
        // Peticion para verifica el estatus del cliente en Port Capital
        public async Task<APIResult<PortCapitalCustomerStatusResponse>> GetPortCapitalCustomerStatus(RequestPortCapitalDataUserForm data)
        {
            var urlLog = "/PCG_FDF_PortCapital/GetCustomerStatus";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(data));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var respuesta = await _HttpClient.SendAsync(request);

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();

                if (refreshSuccess)
                {
                    return await GetPortCapitalCustomerStatus(data);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var resultado = JsonConvert.DeserializeObject<APIResult<PortCapitalCustomerStatusResponse>>(await respuesta.Content.ReadAsStringAsync());

            return resultado;
        }

        // Peticion para obetener una cotización del servicio/paquete cotizado
        public async Task<APIResult<PortCapitalResponse<PortCapitalQuotationResponse?>?>?> GetCalculatedTariffPortCapital(RequestPortCapitalQuotationForm data)
        {
            var urlLog = "/PCG_FDF_PortCapital/PostGetQuotation";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(data));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var respuesta = await _HttpClient.SendAsync(request);

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();

                if (refreshSuccess)
                {
                    return await GetCalculatedTariffPortCapital(data);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var resultado = JsonConvert.DeserializeObject<APIResult<PortCapitalResponse<PortCapitalQuotationResponse>>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<APIResult<PortCapitalCustomerRegisterResponse?>> PostPortCapitalCustomerRegister(RequestPortCapitalCustomerRegisterForm data)
        {
            var urlLog = "/PCG_FDF_PortCapital/CustomerRegister";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(data));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var respuesta = await _HttpClient.SendAsync(request);

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();

                if (refreshSuccess)
                {
                    return await PostPortCapitalCustomerRegister(data);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var resultado = JsonConvert.DeserializeObject<APIResult<PortCapitalCustomerRegisterResponse?>>(await respuesta.Content.ReadAsStringAsync());

            return resultado;
        }

        public async Task<APIResult<PortCapitalBookingResponse>> PostPCB(RequestPortCapitalBookingForm data)
        {
            var urlLog = "/PCG_FDF_PortCapital/PostBooking";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(data));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var respuesta = await _HttpClient.SendAsync(request);

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();

                if (refreshSuccess)
                {
                    return await PostPCB(data);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var resultado = JsonConvert.DeserializeObject<APIResult<PortCapitalBookingResponse>>(await respuesta.Content.ReadAsStringAsync());

            return resultado;
        }

        public async Task<APIResult<PortCapitalPcbContractedResponse>> PostHasPCBContracted(Guid data)
        {
            var urlLog = "/PCG_FDF_PortCapital/HasPCBContracted";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(data));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var respuesta = await _HttpClient.SendAsync(request);

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();

                if (refreshSuccess)
                {
                    return await PostHasPCBContracted(data);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var resultado = JsonConvert.DeserializeObject<APIResult<PortCapitalPcbContractedResponse>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }
        #endregion

        #region EMPTY CONTAINERS REQUESTS
        public async Task<APIResult<CustomerInformation>> GetClientDataEmptyContainers(int CompanyName_ID, int Location_ID)
        {
            var urlLog = "/PCG_FDFContrataciones/PostObtenerInfoClienteVacios";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            var info_client = new KeyValuePair<int, int>(CompanyName_ID, Location_ID);
            request.Content = new StringContent(JsonConvert.SerializeObject(info_client), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await GetClientDataEmptyContainers(CompanyName_ID, Location_ID);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<APIResult<CustomerInformation>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<APIResult<CustomerDocumentsInformation>> GetClientDocumentsDataEmptyContainers()
        {
            var urlLog = "/PCG_FDFContrataciones/PostObtenerInfoDocumentosClienteVacios";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await GetClientDocumentsDataEmptyContainers();
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<APIResult<CustomerDocumentsInformation>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<APIResult<IEnumerable<KeyValue>>> GetServiceUnitsEmptyContainers()
        {
            var urlLog = "/PCG_FDFContrataciones/PostObtenerUnidadesServicioVacios";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await GetServiceUnitsEmptyContainers();
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<APIResult<IEnumerable<KeyValue>>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<APIResult<IEnumerable<KeyValue>>> GetPatentsEmptyContainers(string query)
        {
            var urlLog = $"/PCG_FDFContrataciones/PostObtenerPatentesVacios?query={Uri.EscapeDataString(query)}";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await GetPatentsEmptyContainers(query);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<APIResult<IEnumerable<KeyValue>>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<ApiResponseSimplex<IEnumerable<CompanyInfoTriplet>?>> GetCatCompanyNamesEmptyContainers(string query, int? limit = 15, int? page = 1)
        {
            var urlLog = $"/PCG_FDF_CimaSimplex/GetCatRazonesSocialesVacios?query={Uri.EscapeDataString(query)}&limit={limit}&page={page}";

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, urlLog));

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await GetCatCompanyNamesEmptyContainers(query, limit, page);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                var resultString = respuesta.Content.ReadAsStringAsync();
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<ApiResponseSimplex<IEnumerable<CompanyInfoTriplet>>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<APIResult<IEnumerable<CompanyInfoTriplet>>> GetCompanyNamesEmptyContainers(string query, int? limit = 1000, int? page = 1)
        {
            if(query == null)
            {
                query = "";
            }
            var urlLog = $"/PCG_FDFContrataciones/PostObtenerRazonesSocialesVacios?query={Uri.EscapeDataString(query)}&limit={limit}&page={page}";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await GetCompanyNamesEmptyContainers(query, limit, page);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                var resultString = respuesta.Content.ReadAsStringAsync();
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<APIResult<IEnumerable<CompanyInfoTriplet>>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<APIResult<IEnumerable<SATTableTriplet>>> GetCFDIUsagesEmptyContainers()
        {
            var urlLog = "/PCG_FDFContrataciones/PostObtenerUsoCFDIVacios";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await GetCFDIUsagesEmptyContainers();
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<APIResult<IEnumerable<SATTableTriplet>>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<APIResult<IEnumerable<TaxRegimen>>> GetTaxRegime()
        {
            var urlLog = "/PCG_FDF_CimaSimplex/GetTaxRegime";

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, urlLog));

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await GetTaxRegime();
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<APIResult<IEnumerable<TaxRegimen>>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;

        }

        public async Task<APIResult<IEnumerable<KeyValue>>> GetBanksEmptyContainers()
        {
            var urlLog = "/PCG_FDFContrataciones/PostObtenerBancosVacios";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await GetBanksEmptyContainers();
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<APIResult<IEnumerable<KeyValue>>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<APIResult<IEnumerable<ShippingLineInfoTriplet>>> GetShippingLineEmptyContainers(string operation, int location_id)
        {
            var urlLog = "/PCG_FDFContrataciones/PostObtenerLineaNavieraVacios";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            var requestContent = new Dictionary<string, string>
            {
                { "operation", operation },
                { "location_id", location_id.ToString() }
            };
            request.Content = new FormUrlEncodedContent(requestContent);

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await GetShippingLineEmptyContainers(operation, location_id);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<APIResult<IEnumerable<ShippingLineInfoTriplet>>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<APIResult<IEnumerable<KeyValue>>> GetShipVoyagesEmptyContainers(string query)
        {
            var urlLog = "/PCG_FDFContrataciones/PostObtenerBuquesNavierosVacios";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(query ?? ""), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await GetShipVoyagesEmptyContainers(query);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<APIResult<IEnumerable<KeyValue>>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<APIResult<IEnumerable<EmptyContainerInfoTriplet>>> GetEmptyContainersTypes(ShippingLineInfoTriplet Shipping_Company, int Location_ID, string tipoOperacion)
        {
            var urlLog = "/PCG_FDFContrataciones/PostObtenerTipoContenedorVacios";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            var requestContent = new Dictionary<string, string>
            {
                { "Shipping_Company", Shipping_Company.Value.ToString() },
                { "Location_ID", Location_ID.ToString() },
                {"tipoOperacion", tipoOperacion },
            };
            request.Content = new FormUrlEncodedContent(requestContent);

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await GetEmptyContainersTypes(Shipping_Company, Location_ID, tipoOperacion);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<APIResult<IEnumerable<EmptyContainerInfoTriplet>>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<APIResult<IEnumerable<SATTableTriplet>>> GetPaymentMethodEmptyContainers(int ID_Unit)
        {
            try
            {
                var urlLog = "/PCG_FDFContrataciones/PostObtenerMetodosPagoVacios";

                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
                request.Content = new StringContent(JsonConvert.SerializeObject(ID_Unit.ToString()), Encoding.UTF8, "application/json");

                var respuesta = await _HttpClient.SendAsync(request);
                if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var refreshSuccess = await _AuthService.Refresh();
                    if (refreshSuccess)
                    {
                        return await GetPaymentMethodEmptyContainers(ID_Unit);
                    }
                    else
                    {
                        _navigator.NavigateTo("/login", true, true);
                        return null;
                    }
                }
                else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
                {
                    return null;
                }
                var resultado = JsonConvert.DeserializeObject<APIResult<IEnumerable<SATTableTriplet>>>(await respuesta.Content.ReadAsStringAsync());
                return resultado;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<APIResult<IEnumerable<SATTableTriplet>>> GetPaymentFormEmptyContainers(int ID_Payment_Method)
        {
            var urlLog = "/PCG_FDFContrataciones/PostObtenerFormasPagoVacios";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(ID_Payment_Method.ToString()), Encoding.UTF8, "application/json");

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await GetPaymentFormEmptyContainers(ID_Payment_Method);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<APIResult<IEnumerable<SATTableTriplet>>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }
        public async Task<CSFEntitie?> ParseCustomerDataFromCSFAsync(Stream csfStream)
        {
            var urlLog = "/PCG_FDFContrataciones/PostProcesarDatosCSF";

            try
            {
                var content = new MultipartFormDataContent();
                var fileContent = new StreamContent(csfStream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                content.Add(fileContent, "file", "csf.pdf");

                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog))
                {
                    Content = content
                };

                var response = await _HttpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var refreshSuccess = await _AuthService.Refresh();
                    if (refreshSuccess)
                        return await ParseCustomerDataFromCSFAsync(csfStream);

                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<APIResult<CSFEntitie>>(json);

                if (resultado is not null && resultado.Operation_Succeeded && resultado.Result is not null)
                    return resultado.Result;

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al llamar al endpoint CSF: {ex.Message}");
                return null;
            }
        }


        public async Task<APIResult<ManeuversCustomsDataContainerCimaSimplex<ManeuversCustomsExpoData>?>> GetManeuversCustomersCimaSimplexExpoAsync(ManeuversCustomsCimaSimplexRequest requestGrid)
        {
            try
            {
                var urlLog = "/PCG_FDF_CimaSimplex/GetManeuversCustomsExpo";

                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
                request.Content = new StringContent(JsonConvert.SerializeObject(requestGrid), Encoding.UTF8, "application/json");

                var respuesta = await _HttpClient.SendAsync(request);
                if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var refreshSuccess = await _AuthService.Refresh();
                    if (refreshSuccess)
                    {
                        return await GetManeuversCustomersCimaSimplexExpoAsync(requestGrid);
                    }
                    else
                    {
                        _navigator.NavigateTo("/login", true, true);
                        return null;
                    }
                }

                if (respuesta.StatusCode == HttpStatusCode.BadRequest)
                {
                    return null;
                }
                var resultado = JsonConvert.DeserializeObject<APIResult<ManeuversCustomsDataContainerCimaSimplex<ManeuversCustomsExpoData>?>>(await respuesta.Content.ReadAsStringAsync());
                return resultado;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<APIResult<ManeuversCustomsDataContainerCimaSimplex<ManeuversCustomsImpoData>?>?> GetManeuversCustomersCimaSimplexImpoAsync(ManeuversCustomsCimaSimplexRequest requestGrid)
        {
            try
            {
                var urlLog = "/PCG_FDF_CimaSimplex/GetManeuversCustomsImpo";

                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
                request.Content = new StringContent(JsonConvert.SerializeObject(requestGrid), Encoding.UTF8, "application/json");

                var respuesta = await _HttpClient.SendAsync(request);
                if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var refreshSuccess = await _AuthService.Refresh();
                    if (refreshSuccess)
                    {
                        return await GetManeuversCustomersCimaSimplexImpoAsync(requestGrid);
                    }
                    else
                    {
                        _navigator.NavigateTo("/login", true, true);
                        return null;
                    }
                }

                if (respuesta.StatusCode == HttpStatusCode.BadRequest)
                {
                    return null;
                }

                var resultado = JsonConvert.DeserializeObject<APIResult<ManeuversCustomsDataContainerCimaSimplex<ManeuversCustomsImpoData>?>?>(await respuesta.Content.ReadAsStringAsync());
                return resultado;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }
            return null;
        }

        public async Task<string?> GetClientLastNofiticationEmails(int Client_ID, int Location_ID)
        {
            var urlLog = "/PCG_FDFContrataciones/GetUltimoCorreosNotificacion";
            var formData = new Dictionary<string, string>
            {
                { "Client_ID", Client_ID.ToString() },
                { "Location_ID", Location_ID.ToString() }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog))
            {
                Content = new FormUrlEncodedContent(formData)
            };

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await GetClientLastNofiticationEmails(Client_ID, Location_ID);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var resultado = JsonConvert.DeserializeObject<StringResult>(await respuesta.Content.ReadAsStringAsync());
            return resultado?.Result;
        }

        public async Task<bool?> GetShowPaymentModal(int Client_ID, int Location_ID)
        {
            var urlLog = "/PCG_FDFContrataciones/ObtenerMostrarModalPagos";
            var formData = new Dictionary<string, string>
            {
                { "Client_ID", Client_ID.ToString() },
                { "Location_ID", Location_ID.ToString() }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog))
            {
                Content = new FormUrlEncodedContent(formData)
            };

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await GetShowPaymentModal(Client_ID, Location_ID);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var resultado = JsonConvert.DeserializeObject<BooleanResult>(await respuesta.Content.ReadAsStringAsync());
            return resultado?.Result;
        }

        public async Task<APIResult<string>> GetDownloadZipAsync(string bookingFdf, string operationType)
        {
            try
            {
                var queryParams = new Dictionary<string, string>
                {
                    { "bookingFdf", bookingFdf },
                    { "operationType", operationType },
                };

                var queryString = string.Join("&", queryParams.Select(parameter => $"{parameter.Key}={parameter.Value}"));
                var url = $"{_HttpClient.BaseAddress}PCG_FDF_CimaSimplex/GetManeuversZip?{queryString}";

                var respuesta = await _HttpClient.GetAsync(url);
                if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var refreshSuccess = await _AuthService.Refresh();
                    if (refreshSuccess)
                    {
                        return await GetDownloadZipAsync(bookingFdf, operationType);
                    }
                    else
                    {
                        _navigator.NavigateTo("/login", true, true);
                        return null;
                    }
                }

                if (respuesta.StatusCode == HttpStatusCode.BadRequest)
                {
                    return null;
                }
                var resultado = JsonConvert.DeserializeObject<APIResult<string>>(await respuesta.Content.ReadAsStringAsync());
                return resultado;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<APIResult<string>> GetDownloadZipInvoiceAsync(int? facturaencabezado)
        {
            try
            {
                var urlLog = $"/PCG_FDF_CimaSimplex/GetDownloadZipInvoiceAsync?facturaencabezado={facturaencabezado}";
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, urlLog));

                var respuesta = await _HttpClient.SendAsync(request);
                if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var refreshSuccess = await _AuthService.Refresh();
                    if (refreshSuccess)
                    {
                        return await GetDownloadZipInvoiceAsync(facturaencabezado);
                    }
                    else
                    {
                        _navigator.NavigateTo("/login", true, true);
                        return null;
                    }
                }

                if (respuesta.StatusCode == HttpStatusCode.BadRequest)
                {
                    return null;
                }
                var resultado = JsonConvert.DeserializeObject<APIResult<string>>(await respuesta.Content.ReadAsStringAsync());
                return resultado;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<APIResult<StringBase64Response>> DownloadInvoiceAsync(string invoiceId, string tipoOperacion)
        {
            try
            {
                var queryParams = new Dictionary<string, string>
                {
                    { "invoiceId", invoiceId.ToString() },
                    {"tipoOperacion", tipoOperacion },
                };

                var queryString = string.Join("&", queryParams.Select(parameter => $"{parameter.Key}={parameter.Value}"));

                var url = $"{_HttpClient.BaseAddress}PCG_FDF_CimaSimplex/DownloadIncoices?{queryString}";

                var respuesta = await _HttpClient.GetAsync(url);
                if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var refreshSuccess = await _AuthService.Refresh();
                    if (refreshSuccess)
                    {
                        return null;
                    }
                    else
                    {
                        _navigator.NavigateTo("/login", true, true);
                        return null;
                    }
                }

                if (respuesta.StatusCode == HttpStatusCode.BadRequest)
                {
                    return null;
                }
                var resultado = JsonConvert.DeserializeObject<APIResult<StringBase64Response>>(await respuesta.Content.ReadAsStringAsync());
                return resultado;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<Stream?> DownloadEIRContainer(string eir, string tipoOperacion)
        {
            try
            {
                var queryParams = new Dictionary<string, string>
                {
                    { "eir", eir.ToString() },
                    {"tipoOperacion", tipoOperacion },
                };

                var queryString = string.Join("&", queryParams.Select(parameter => $"{parameter.Key}={parameter.Value}"));

                var url = $"{_HttpClient.BaseAddress}PCG_FDF_CimaSimplex/DownloadEIRContainer?{queryString}";

                var respuesta = await _HttpClient.GetAsync(url);
                if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var refreshSuccess = await _AuthService.Refresh();
                    if (refreshSuccess)
                    {
                        return null;
                    }
                    else
                    {
                        _navigator.NavigateTo("/login", true, true);
                        return null;
                    }
                }

                if (respuesta.StatusCode == HttpStatusCode.BadRequest)
                {
                    return null;
                }
                var resultado = await respuesta.Content.ReadAsStreamAsync();
                return resultado;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        
        public async Task<APIResult<Guid?>> GetBookingByFolioAsync(string bookingFdf)
        {
            try
            {
                var urlLog = "/PCG_FDFContrataciones/GetBooking";
                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
                request.Content = new StringContent(JsonConvert.SerializeObject(bookingFdf), Encoding.UTF8, "application/json");

                var respuesta = await _HttpClient.SendAsync(request);
                if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var refreshSuccess = await _AuthService.Refresh();
                    if (refreshSuccess)
                    {
                        return await GetBookingByFolioAsync(bookingFdf);
                    }
                    else
                    {
                        _navigator.NavigateTo("/login", true, true);
                        return null;
                    }
                }

                if (respuesta.StatusCode == HttpStatusCode.BadRequest)
                {
                    return null;
                }
                var resultado = JsonConvert.DeserializeObject<APIResult<Guid?>>(await respuesta.Content.ReadAsStringAsync());
                return resultado;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<APIResult<Guid?>> GetMotivoSuspensionById(int folioID)
        {
            try
            {
                var urlLog = "/PCG_FDF_CimaSimplex/GetMotivoSuspensionById";
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, urlLog));

                var respuesta = await _HttpClient.SendAsync(request);
                if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var refreshSuccess = await _AuthService.Refresh();
                    if (refreshSuccess)
                    {
                        return await GetMotivoSuspensionById(folioID);
                    }
                    else
                    {
                        _navigator.NavigateTo("/login", true, true);
                        return null;
                    }
                }

                if (respuesta.StatusCode == HttpStatusCode.BadRequest)
                {
                    return null;
                }
                var resultado = JsonConvert.DeserializeObject<APIResult<Guid?>>(await respuesta.Content.ReadAsStringAsync());
                return resultado;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<APIResult<RazonSocial>> GetRazonSocial(int id)
        {
            try
            {
                var urlLog = $"/PCG_FDF_CimaSimplex/GetRazonSocial?idRazon={id}";
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, urlLog));

                var response = await _HttpClient.SendAsync(request);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var refreshSuccess = await _AuthService.Refresh();
                    if (refreshSuccess)
                    {
                        return await GetRazonSocial(id);
                    }
                    else
                    {
                        _navigator.NavigateTo("/login", true, true);
                        return new APIResult<RazonSocial> { Operation_Succeeded = false };
                    }
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return new APIResult<RazonSocial> { Operation_Succeeded = false };
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<APIResult<RazonSocial>>(jsonResponse);
                return result ?? new APIResult<RazonSocial> { Operation_Succeeded = false };
            }
            catch (Exception ex)
            {
                return new APIResult<RazonSocial> { Operation_Succeeded = false };
            }
        }
        public async Task<APIResult<CountriesResponse>> GetCountries()
        {
            var urlLog = "/PCG_FDF_CimaSimplex/GetCountries";

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, urlLog));

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await GetCountries();
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = JsonConvert.DeserializeObject<APIResult<CountriesResponse>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }
        public async Task<APIResult<ApiErrorResponse>> PutEliminarSocial(int id)
        {
            try
            {
                var urlLog = $"/PCG_FDF_CimaSimplex/PutEliminarSocial?idRazon={id}";
                var request = new HttpRequestMessage(HttpMethod.Put, new Uri(_HttpClient.BaseAddress!, urlLog));

                var response = await _HttpClient.SendAsync(request);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var refreshSuccess = await _AuthService.Refresh();
                    if (refreshSuccess)
                    {
                        return await PutEliminarSocial(id);
                    }
                    else
                    {
                        _navigator.NavigateTo("/login", true, true);
                        return new APIResult<ApiErrorResponse> { Operation_Succeeded = false };
                    }
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return new APIResult<ApiErrorResponse> { Operation_Succeeded = false };
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<APIResult<ApiErrorResponse>>(jsonResponse);
                return result;
            }
            catch (Exception ex)
            {
                return new APIResult<ApiErrorResponse> { Operation_Succeeded = false };
            }
        }
        #endregion

        #region CIMA SIMPLEX PAYMENTS
        public async Task<APIResult<PaymentInvocieHeaderResponse?>> GetPaymentInvoce(PaymentInvoceRequest data)
        {
            var urlLog = "/PCG_FDF_CimaSimplex/GetPaymentInvoce";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(data));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var respuesta = await _HttpClient.SendAsync(request);

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();

                if (refreshSuccess)
                {
                    return await GetPaymentInvoce(data);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var resultado = JsonConvert.DeserializeObject<APIResult<PaymentInvocieHeaderResponse?>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<APIResult<IEnumerable<PaymentConceptsResponse>?>> GetPaymentConcepts()
        {
            var urlLog = "/PCG_FDF_CimaSimplex/GetPaymentConcepts";

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, urlLog));
            var respuesta = await _HttpClient.SendAsync(request);

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();

                if (refreshSuccess)
                {
                    return await GetPaymentConcepts();
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var resultado = JsonConvert.DeserializeObject<APIResult<IEnumerable<PaymentConceptsResponse>?>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<APIResult<PaymentReferenceResponse?>> PostPaymentReference(PaymentReferenceRequest data)
        {
            var urlLog = "/PCG_FDF_CimaSimplex/PostPaymentReference";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(data));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var respuesta = await _HttpClient.SendAsync(request);

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();

                if (refreshSuccess)
                {
                    return await PostPaymentReference(data);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var resultado = JsonConvert.DeserializeObject<APIResult<PaymentReferenceResponse?>>(await respuesta.Content.ReadAsStringAsync());
            return resultado;
        }

        public async Task<APIResult<bool?>> DeleteLastReference(string referenceID)
        {
            try
            {
                var urlLog = "/PCG_FDF_CimaSimplex/DeletePaymentReference";
                
                var references = referenceID.Split(',');
                var lastReferenceID = references.Select(id => int.Parse(id)).Max().ToString();
                
                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog))
                {
                    Content = new StringContent(JsonConvert.SerializeObject(lastReferenceID), Encoding.UTF8, "application/json")
                };

                var respuesta = await _HttpClient.SendAsync(request);

                if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var refreshSuccess = await _AuthService.Refresh();

                    if (refreshSuccess)
                        return await DeleteLastReference(referenceID);

                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
                else if (!respuesta.IsSuccessStatusCode)
                {
                    return null;
                }

                var json = await respuesta.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<APIResult<bool?>>(json);
                return resultado;
            } catch (Exception ex)
            {
                var error = ex.Message;
                return new APIResult<bool?>();
            }
            
        }

        public async Task<APIResult<bool?>> SendPaymentVoucher(PaymentDownloadRequest downloadData)
        {
            var urlLog = "/PCG_FDF_CimaSimplex/PostSendPaymentVoucher";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog))
            {
                Content = new StringContent(JsonConvert.SerializeObject(downloadData), Encoding.UTF8, "application/json")
            };

            var respuesta = await _HttpClient.SendAsync(request);

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();

                if (refreshSuccess)
                    return await SendPaymentVoucher(downloadData);

                _navigator.NavigateTo("/login", true, true);
                return null;
            }
            else if (!respuesta.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await respuesta.Content.ReadAsStringAsync();
            var resultado = JsonConvert.DeserializeObject<APIResult<bool?>>(json);

            return resultado;
        }

        public async Task<Stream?> PostDownloadVoucherPayment(PaymentDownloadRequest downloadData)
        {
            var urlLog = "/PCG_FDF_CimaSimplex/PostDownloadPaymentVoucher";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog))
            {
                Content = new StringContent(JsonConvert.SerializeObject(downloadData), Encoding.UTF8, "application/json")
            };

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await PostDownloadVoucherPayment(downloadData);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }
            var resultado = await respuesta.Content.ReadAsStreamAsync();

            return resultado;
        }

        public async Task<Stream?> PostDownloadPaymentVoucherByFileId(PaymentDownloadByFileIdRequest data)
        {
            var urlLog = "/PCG_FDF_CimaSimplex/PostDownloadPaymentVoucherByFileId";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog))
            {
                Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
            };

            var respuesta = await _HttpClient.SendAsync(request);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();
                if (refreshSuccess)
                {
                    return await PostDownloadPaymentVoucherByFileId(data);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var resultado = await respuesta.Content.ReadAsStreamAsync();
            return resultado;
        }

        public async Task<Stream?> PostDownloadEmptyInvoiceZip(int invoiceID)
        {
            var urlLog = "/PCG_FDF_CimaSimplex/PostDownloadInvoiceZip";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(invoiceID));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var respuesta = await _HttpClient.SendAsync(request);

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();

                if (refreshSuccess)
                {
                    return await PostDownloadEmptyInvoiceZip(invoiceID);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var result = await respuesta.Content.ReadAsStreamAsync();
            return result;
        }
        public async Task<Stream?> PostDownloadInvoiceZip(int invoiceID)
        {
            var urlLog = "/PCG_FDF_CimaSimplex/PostDownloadInvoiceZip2";

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, urlLog));
            request.Content = new StringContent(JsonConvert.SerializeObject(invoiceID));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var respuesta = await _HttpClient.SendAsync(request);

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshSuccess = await _AuthService.Refresh();

                if (refreshSuccess)
                {
                    return await PostDownloadInvoiceZip(invoiceID);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (respuesta.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var result = await respuesta.Content.ReadAsStreamAsync();
            return result;
        }

        public async Task<APIResult<StoreCustomerResponse?>> PostStoreCustomer(StoreCustomerRequest data)
        {

            var url = "/PCG_FDF_CimaSimplex/PostStoreCustomer";

            using var form = new MultipartFormDataContent();

           
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress!, url));
            request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            var response = await _HttpClient.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshed = await _AuthService.Refresh();
                if (refreshed)
                {
                    return await PostStoreCustomer(data);
                }
                else
                {
                    _navigator.NavigateTo("/login", true, true);
                    return null;
                }
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<APIResult<StoreCustomerResponse?>>(jsonResponse);
        }
        public async Task<APIResult<PutCustomerResponse>> PutUpdateCustomer(PutCustomerRequest data)
        {
            try
            {
                var url = "/PCG_FDF_CimaSimplex/PutUpdateCustomer";
                var request = new HttpRequestMessage(HttpMethod.Put, new Uri(_HttpClient.BaseAddress!, url));
                request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

                // Agregar logging para verificar la URL y el contenido
                Console.WriteLine($"BaseAddress: {_HttpClient.BaseAddress}");
                Console.WriteLine($"Request URL: {new Uri(_HttpClient.BaseAddress!, url)}");
                Console.WriteLine($"Request Content: {JsonConvert.SerializeObject(data)}");

                var response = await _HttpClient.SendAsync(request);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var refreshed = await _AuthService.Refresh();
                    if (refreshed)
                    {
                        return await PutUpdateCustomer(data);
                    }
                    else
                    {
                        _navigator.NavigateTo("/login", true, true);
                        return null;
                    }
                }
                else if (response.StatusCode != HttpStatusCode.OK)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error response: {response.StatusCode} - {errorContent}");
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<APIResult<PutCustomerResponse>>(jsonResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en PutUpdateCustomer: {ex.Message}");
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<APIResult<ManeuversContainerInfo?>> GetInfoContainerCimaSimplex(int webRequestId, string tipoOperacion)
        {
            try
            {
                var queryParams = new Dictionary<string, string>
        {
            { "operation", tipoOperacion }
        };

                var queryString = string.Join("&", queryParams.Select(parameter => $"{parameter.Key}={parameter.Value}"));
                var url = $"/PCG_FDF_CimaSimplex/GetInfoContainerCimaSimplex/{webRequestId}?{queryString}";

                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, url));

                var response = await _HttpClient.SendAsync(request);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var refreshed = await _AuthService.Refresh();
                    if (refreshed)
                    {
                        return await GetInfoContainerCimaSimplex(webRequestId, tipoOperacion);
                    }
                    else
                    {
                        _navigator.NavigateTo("/login", true, true);
                        return new APIResult<ManeuversContainerInfo?> { Operation_Succeeded = false };
                    }
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return new APIResult<ManeuversContainerInfo?> { Operation_Succeeded = false };
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<APIResult<ManeuversContainerInfo?>>(jsonResponse);
                return result ?? new APIResult<ManeuversContainerInfo?> { Operation_Succeeded = false };
            }
            catch (Exception ex)
            {
                return new APIResult<ManeuversContainerInfo?> { Operation_Succeeded = false };
            }
        }
        public async Task<APIResult<ManeuversContainerInfoExpo?>> GetInfoContainerCimaSimplexExpo(int webRequestId, string tipoOperacion)
        {
            try
            {
                var queryParams = new Dictionary<string, string>
        {
            { "operation", tipoOperacion }
        };

                var queryString = string.Join("&", queryParams.Select(parameter => $"{parameter.Key}={parameter.Value}"));
                var url = $"/PCG_FDF_CimaSimplex/GetInfoContainerCimaSimplexExpo/{webRequestId}?{queryString}";

                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_HttpClient.BaseAddress!, url));

                var response = await _HttpClient.SendAsync(request);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var refreshed = await _AuthService.Refresh();
                    if (refreshed)
                    {
                        return await GetInfoContainerCimaSimplexExpo(webRequestId, tipoOperacion);
                    }
                    else
                    {
                        _navigator.NavigateTo("/login", true, true);
                        return new APIResult<ManeuversContainerInfoExpo?> { Operation_Succeeded = false };
                    }
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return new APIResult<ManeuversContainerInfoExpo?> { Operation_Succeeded = false };
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<APIResult<ManeuversContainerInfoExpo?>>(jsonResponse);
                return result ?? new APIResult<ManeuversContainerInfoExpo?> { Operation_Succeeded = false };
            }
            catch (Exception ex)
            {
                return new APIResult<ManeuversContainerInfoExpo?> { Operation_Succeeded = false };
            }
        }

        #endregion
    }
}



