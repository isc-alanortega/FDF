using PCG_FDF.Data.DataAccess;
using PCG_FDF.Data.Entities;
using PCG_ENTITIES.PCG_FDF.UtilityEntities;
using PCG_ENTITIES.PCG_FDF;
using PCG_FDF.Utility;
using PCG_ENTITIES.PCG_FDF.UtilityEntities.Tariffs;
using PCG_ENTITIES.Enums;

namespace PCG_FDF.Data.ComponentDI.Quotation
{
    public class ReadOnlyQuotationData
    {
        // Initialized
        private bool HasBeenInitialized = false;
        // Saved
        private bool HasSaved = true;
        // Calculation in progress
        private bool Is_Calculating = false;
        // Saving in progress
        private bool Is_Saving = false;
        // Money Kind
        private EMoney money_kind = EMoney.USD;
        // Controls global state of the components
        private bool Global_Disable = true;
        private int language;
        // Unique Identifier that maps this quotation
        private Guid Quotation_GUID;
        private DateTime Creation_Date;
        private bool PDF_Available = false;
        private bool Pay_CIMA_Coins = false;
        private StaticShoppingCart Static_Shopping_Cart;
        // Stores data related to total prices
        private QTotals Totals = new QTotals();
        // Dictionary ServiceID, HashSet<ElementID>
        // Saves which service is associated with which elements
        private IDictionary<int, HashSet<int>> serviceElement_pairs = new Dictionary<int, HashSet<int>>();
        // Dictionary ElementID, ServiceData
        // ServiceData saves fields for all elements
        private IDictionary<int, ReadOnlyElement> quotation_data = new Dictionary<int, ReadOnlyElement>();
        // Dictionary ServiceID, bool
        // Saves the state of each service, if all required data has been filled
        private IDictionary<int, bool> service_data_ready = new Dictionary<int, bool>();
        // Dictionary <ServiceID, Dictionary<SubserviceID, bool>>
        // Stores subservices selected for each service
        private IDictionary<int, IDictionary<int, bool>> subservices_data = new Dictionary<int, IDictionary<int, bool>>();
        // Dictionary ServiceID, Tarifa
        // Stores the tariff of each service
        private IDictionary<int, ServiceTariff> tarifas_servicios = new Dictionary<int, ServiceTariff>();
        // Dictionary <PackageId, Tarifa>
        // Stores a relationship of package to its commercial tariff
        private IDictionary<int, PackageTariff> tarifas_paquete = new Dictionary<int, PackageTariff>();
        private IList<int> service_ids = new List<int>();
        private IList<int> package_ids = new List<int>();
        // Action that triggers a StateHasChanged event to rerender the component
        public event Action OnChange;
        private readonly PCG_FDF_DB DATA_ACCESS;
        private readonly WhiteLabelManager WhiteLabel;

        /// <summary>
        /// Dispara el evento de StateHasChanged y notifica al componente principal
        /// </summary>
        private void NotifyStateChanged() => OnChange?.Invoke();

        public void ResetSelf()
        {
            HasBeenInitialized = false;
            Quotation_GUID = Guid.Empty;
            Static_Shopping_Cart.ResetSelf();
            serviceElement_pairs.Clear();
            quotation_data.Clear();
            service_data_ready.Clear();
            subservices_data.Clear();
            tarifas_servicios.Clear();
            tarifas_paquete.Clear();
            service_ids.Clear();
            package_ids.Clear();
        }

        public ReadOnlyQuotationData(PCG_FDF_DB data_access, WhiteLabelManager whiteLabel)
        {
            DATA_ACCESS = data_access;
            WhiteLabel = whiteLabel;
        }

        public void Initialize(MiddlewareQuotation saved_data, MiddlewareShoppingCart cart_data, string language)
        {
            Quotation_GUID = saved_data.Quotation_GUID;
            Creation_Date = saved_data.Creation_Date;
            money_kind = saved_data.money_kind;
            this.language = saved_data.language;
            Static_Shopping_Cart = new StaticShoppingCart(cart_data);
            Pay_CIMA_Coins = saved_data.Pay_CIMA_Coins;
            Totals = saved_data.Totals;
            serviceElement_pairs = saved_data.serviceElement_pairs;
            quotation_data = saved_data.quotation_data.ToDictionary(middleware => middleware.Key, middleware => new ReadOnlyElement(middleware.Value, language));
            service_data_ready = saved_data.service_data_ready;
            subservices_data = saved_data.subservices_data;
            tarifas_servicios = saved_data.tarifas_servicios;
            tarifas_paquete = saved_data.tarifas_paquete;
            service_ids = saved_data.service_ids;
            package_ids = saved_data.package_ids;
            PDF_Available = AllReady();

            HasBeenInitialized = true;

            NotifyStateChanged();
        }

        private bool AllReady()
        {
            return service_data_ready.Values.All(ready => ready);
        }

        public StaticShoppingCart GetQuotationCart()
        {
            return Static_Shopping_Cart;
        }

        public bool GetQuotationInitialized()
        {
            return HasBeenInitialized;
        }

        public EMoney GetQuotationMoneyKind()
        {
            return money_kind;
        }

        public bool GetQuotationLocked()
        {
            return Global_Disable;
        }

        public bool GetUseCIMACoins()
        {
            return Pay_CIMA_Coins;
        }

        public bool GetQuotationIsCalculating()
        {
            return Is_Calculating;
        }

        public bool GetQuotationPDFAvailable()
        {
            return PDF_Available;
        }

        public bool GetQuotationIsSaving()
        {
            return Is_Saving;
        }

        public bool GetQuotationHasSaved()
        {
            return HasSaved;
        }

        public void CreateQuotationCart(ShoppingCartContainer cart)
        {
            Static_Shopping_Cart = new StaticShoppingCart(cart);
        }

        public int GetQuotationLanguage()
        {
            return language;
        }

        public DateTime GetQuotationCreationDate()
        {
            return Creation_Date;
        }

        public async Task<Stream?> GetQuotationPDF()
        {
            var stream = await DATA_ACCESS.PostDownloadPDF(GetQuotationGUID(), LanguageUtil.getCurrentCulture(), WhiteLabel.Current_Page);
            return stream;
        }

        public bool ServiceComplete(int serviceID)
        {
            if (service_data_ready.TryGetValue(serviceID, out _))
            {
                return service_data_ready[serviceID];
            }
            else
            {
                return false;
            }
        }

        public IEnumerable<bool?> PackageComplete(IList<ServiciosPaqueteEditable> services)
        {
            IList<bool?> result = new List<bool?>();
            foreach (var service in services)
            {
                if (service.Active)
                {
                    result.Add(ServiceComplete(service.Id));
                }
                else
                {
                    result.Add(null);
                }
            }
            return result;
        }

        public Guid GetQuotationGUID()
        {
            return Quotation_GUID;
        }

        public QTotals GetTotals()
        {
            return Totals;
        }

        public bool HasCommercial()
        {
            var com_packages = tarifas_paquete.Values.Any(package => package.Tarifa_Comercial is not null);
            var com_services = tarifas_servicios.Values.Any(service => service.Service_Tariff is not null && service.Service_Tariff.Tarifa_Comercial is not null);
            return com_packages || com_services;
        }

        public IDictionary<int, ServiceTariff> GetServiceTariffs()
        {
            return tarifas_servicios;
        }

        public bool CheckSubservicesExist(int serviceId)
        {
            return subservices_data.TryGetValue(serviceId, out _);
        }

        public void AddSubservices(int serviceID, IEnumerable<int> subserviceIDs)
        {
            subservices_data[serviceID] = new Dictionary<int, bool>();
            foreach (var subserviceID in subserviceIDs)
            {
                subservices_data[serviceID].Add(subserviceID, false);
            }
        }

        public bool GetSubserviceData(int serviceID, int subserviceID)
        {
            return subservices_data[serviceID][subserviceID];
        }

        public IDictionary<int, PackageTariff> GetPackagesTariffs()
        {
            return tarifas_paquete;
        }

        public void SetServiceIDs(IEnumerable<int> ids)
        {
            service_ids = ids.ToList();
        }

        public IEnumerable<int> GetServiceIds()
        {
            return service_ids;
        }

        public void SetPackageIDs(IEnumerable<int> ids)
        {
            package_ids = ids.ToList();
        }

        public IEnumerable<int> GetPackageIds()
        {
            return package_ids;
        }

        public HashSet<int> GetElements(IEnumerable<int> service_ids)
        {
            return service_ids.SelectMany(service_id => serviceElement_pairs[service_id]).ToHashSet();
        }

        public IDictionary<int, ReadOnlyElement> GetQuotationData()
        {
            return quotation_data;
        }

        public ReadOnlyElement GetElementQuotationData(int elementID)
        {
            return quotation_data[elementID];
        }

        public bool ElementDataExists(int elementID)
        {
            return quotation_data.ContainsKey(elementID);
        }

        public bool ServiceDataExists(IEnumerable<int> service_ids)
        {
            return service_ids.All(service_id => serviceElement_pairs.ContainsKey(service_id));
        }

        public void AddServicePair(int serviceID, int elementID)
        {
            if (serviceElement_pairs.ContainsKey(serviceID))
            {
                serviceElement_pairs[serviceID].Add(elementID);
            }
            else
            {
                serviceElement_pairs[serviceID] = new HashSet<int> { elementID };
            }
        }
    }
}
