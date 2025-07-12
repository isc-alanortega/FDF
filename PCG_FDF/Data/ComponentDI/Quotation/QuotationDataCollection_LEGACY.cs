using PCG_ENTITIES.PCG;
using PCG_FDF.Data.DataAccess;
using PCG_FDF.Data.Entities;
using System.Data;
using PCG_ENTITIES.PCG_FDF.UtilityEntities;
using PCG_ENTITIES.PCG_FDF;
using PCG_FDF.Utility;
using PCG_ENTITIES.PCG_FDF.UtilityEntities.Tariffs;
using Microsoft.AspNetCore.Components.Authorization;
using PCG_FDF.Data.ComponentDI.AuthManagement;
using PCG_FDF.Data.ComponentDI.Quotation;
using PCG_ENTITIES.Enums;

namespace PCG_FDF.Data.ComponentDI
{
    public class QuotationDataCollection_LEGACY
    {
        // Initialized
        private bool HasBeenInitialized = false;
        // Saved
        private bool HasSaved = false;
        // Calculation in progress
        private bool Is_Calculating = false;
        /// <summary>
        /// Indicates the page this quotation was made from
        /// </summary>
        private EPages Quotation_Page { get; set; }
        /// <summary>
        /// Indicates the page this quotation was made from
        /// </summary>
        private string Quotation_URI { get; set; }
        // Saving in progress
        private bool Is_Saving = false;
        // Money Kind
        private EMoney money_kind = EMoney.USD;
        // Controls global state of the components
        private bool Global_Disable = false;
        private int language;
        // Unique Identifier that maps this quotation
        private Guid Quotation_GUID;
        private DateTime Creation_Date;
        private int User_ID;
        private int Client_ID;
        private bool PDF_Available = false;
        private bool Pay_CIMA_Coins = false;
        private decimal Coins_Rate = 0;
        private bool All_Valid_Tariffs = false;
        private StaticShoppingCart Static_Shopping_Cart;
        // Stores data related to total prices
        private QTotals Totals = new();
        // Dictionary ServiceID, HashSet<ElementID>
        // Saves which service is associated with which elements
        private IDictionary<int, HashSet<int>> serviceElement_pairs = new Dictionary<int, HashSet<int>>();
        // Dictionary ElementID, ServiceData
        // ServiceData saves fields for all elements
        private IDictionary<int, ElementQuotationData> quotation_data = new Dictionary<int, ElementQuotationData>();
        // Dictionary ServiceID, bool
        // Saves the state of each service, if all required data has been filled
        private IDictionary<int, bool> service_data_ready = new Dictionary<int, bool>();
        // Dictionary ServiceID, IEnumerable<CamposTarifarios>
        // Saves the tariff fields of each service
        private IDictionary<int, IEnumerable<ServicioCamposTarifariosEntidad>> campos_tarifarios = new Dictionary<int, IEnumerable<ServicioCamposTarifariosEntidad>>();
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

        private IDictionary<int, int> packages_reverselookup = new Dictionary<int, int>();
        private IDictionary<int, HashSet<int>> package_services_ids = new Dictionary<int, HashSet<int>>();
        /// <summary>
        /// Action that triggers a StateHasChanged event to rerender the component
        /// </summary>
        public event Action OnChange;
        private readonly PCG_FDF_DB DATA_ACCESS;
        private readonly ShoppingCartContainer Shopping_Cart_Hook;
        private readonly WhiteLabelManager WhiteLabel;
        private readonly AuthenticationStateProvider AuthProvider;

        public QuotationDataCollection_LEGACY(PCG_FDF_DB database, ShoppingCartContainer shopping_cart_hook, WhiteLabelManager whiteLabel, AuthenticationStateProvider auth_provider)
        {
            DATA_ACCESS = database;
            Shopping_Cart_Hook = shopping_cart_hook;
            WhiteLabel = whiteLabel;
            Quotation_Page = WhiteLabel.Current_Page;
            Quotation_URI = WhiteLabel.GetPageURI();
            AuthProvider = auth_provider;
        }

        public async Task SetCIMACoins(bool value)
        {
            Pay_CIMA_Coins = value;
            if (value)
            {
                await UpdateTotals();
            }
            else
            {
                Totals.Spent_CIMA_Coins = 0;
                Totals.CIMA_Coins_Discount = 0;
                await UpdateTotals();
            }
            await SaveQuotation();
            NotifyStateChanged();
        }

        public void ResetSelf()
        {
            HasBeenInitialized = false;
            HasSaved = false;
            Is_Calculating = false;
            Is_Saving = false;
            Global_Disable = false;
            PDF_Available = false;
            Pay_CIMA_Coins = false;
            Totals = new QTotals();
            serviceElement_pairs.Clear();
            foreach (var item in quotation_data.Values)
            {
                item.Dispose();
            }
            quotation_data.Clear();
            service_data_ready.Clear();
            campos_tarifarios.Clear();
            subservices_data.Clear();
            tarifas_servicios.Clear();
            tarifas_paquete.Clear();
            service_ids.Clear();
            package_ids.Clear();
            packages_reverselookup.Clear();
            Quotation_GUID = Guid.Empty;
            serviceElement_pairs.Clear();
            package_services_ids.Clear();
            quotation_data.Clear();
            service_data_ready.Clear();
            subservices_data.Clear();
            tarifas_servicios.Clear();
            tarifas_paquete.Clear();
            service_ids.Clear();
            package_ids.Clear();
        }

        public async Task Initialize()
        {
            Creation_Date = DateTime.UtcNow;
            money_kind = await GetMoneyKind();
            Coins_Rate = await GetCoinsRate(money_kind);
            Quotation_GUID = Guid.NewGuid();
            var user_context = await ((ApiAuthenticationStateProvider)AuthProvider).GetAuthenticationStateAsync();
            User_ID = Convert.ToInt32(user_context.User.Claims.FirstOrDefault(claims => claims.Type == "User ID")!.Value);
            Client_ID = Convert.ToInt32(user_context.User.Claims.FirstOrDefault(claims => claims.Type == "Client ID")!.Value);
            if (LanguageUtil.getCurrentCulture() == "EN")
            {
                language = 1;
            }
            else
            {
                language = 2;
            }
            HasBeenInitialized = true;
        }

        public void Initialize(MiddlewareQuotation saved_data, MiddlewareShoppingCart cart_data)
        {
            HasSaved = true;
            Quotation_GUID = saved_data.Quotation_GUID;
            Creation_Date = saved_data.Creation_Date;
            money_kind = saved_data.money_kind;
            User_ID = saved_data.User_ID;
            Client_ID = saved_data.Client_ID;
            if (LanguageUtil.getCurrentCulture() == "EN")
            {
                language = 1;
            }
            else
            {
                language = 2;
            }
            Static_Shopping_Cart = new StaticShoppingCart(cart_data);
            Pay_CIMA_Coins = saved_data.Pay_CIMA_Coins;
            Coins_Rate = saved_data.Coins_Rate;
            Totals = saved_data.Totals;
            serviceElement_pairs = saved_data.serviceElement_pairs;
            quotation_data = saved_data.quotation_data.ToDictionary(middleware => middleware.Key, middleware => new ElementQuotationData(middleware.Value, this));
            service_data_ready = saved_data.service_data_ready;
            campos_tarifarios = saved_data.campos_tarifarios;
            subservices_data = saved_data.subservices_data;
            service_ids = saved_data.service_ids;
            package_ids = saved_data.package_ids;
            packages_reverselookup = Static_Shopping_Cart.GetPackages().Select(package => package.Value).SelectMany(package => package.Servicios).Where(service => service.Value.Active).ToDictionary(service => service.Value.Id, service => service.Value.ID_Paquete);
            PDF_Available = AllReady();
            package_services_ids = saved_data.package_services_ids;

            HasBeenInitialized = true;

            NotifyStateChanged();
        }

        /// <summary>
        /// Dispara el evento de StateHasChanged y notifica al componente principal
        /// </summary>
        private void NotifyStateChanged() => OnChange?.Invoke();

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

        public bool GetQuotationFullyValid()
        {
            return All_Valid_Tariffs;
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

        public void SetPackageServicesIds(IDictionary<int, HashSet<int>> Package_Services_Ids)
        {
            package_services_ids = Package_Services_Ids;
        }

        public IDictionary<int, HashSet<int>> GetPackageServicesIds()
        {
            return package_services_ids;
        }

        public void CreateQuotationCart(ShoppingCartContainer cart)
        {
            Static_Shopping_Cart = new StaticShoppingCart(cart);
        }

        public Guid GetQuotationGUID()
        {
            return Quotation_GUID;
        }

        public QTotals GetTotals()
        {
            return Totals;
        }

        public int GetQuotationLanguage()
        {
            return language;
        }

        public DateTime GetQuotationCreationDate()
        {
            return Creation_Date;
        }

        public int GetQuotationUserID()
        {
            return User_ID;
        }

        public bool GetHasInitialized()
        {
            return HasBeenInitialized;
        }

        public int GetQuotationClientID()
        {
            return Client_ID;
        }

        public decimal GetQuotationCoinsRate()
        {
            return Coins_Rate;
        }

        public IDictionary<int, bool> GetServiceDataReady()
        {
            return service_data_ready;
        }

        public bool AllReady()
        {
            return service_data_ready.Values.All(ready => ready);
        }

        public string GetQuotationURI()
        {
            return Quotation_URI;
        }

        public EPages GetQuotationPage()
        {
            return Quotation_Page;
        }

        public async Task<MiddlewareQuotation> GetAllData(bool is_update)
        {
            return await Task.Run(() => GeneratorsUtil.GenerateQuotationObject(this, is_update));
        }

        private void ReCalculateAllValidTariffs()
        {
            var packages_conditions = tarifas_paquete.Values.All(tariff => tariff.Valid_Tariff);
            var services_conditions = tarifas_servicios.Values.All(tariff => tariff.Valid_Tariff);
            All_Valid_Tariffs = packages_conditions && services_conditions;
        }

        public async Task SaveQuotation()
        {
            if (!Is_Saving && All_Valid_Tariffs)
            {
                Is_Saving = true;
                NotifyStateChanged();
                var data = await GetAllData(HasSaved);
                data.read_only = false;
                var result = await DATA_ACCESS.PostSerializeQuotation(data);
                if (result is not null && result.Success)
                {
                    HasSaved = true;
                }// ELSE ERROR TODO
                Is_Saving = false;
                NotifyStateChanged();
            }
        }

        public async Task SaveFullQuotation()
        {
            if (!Global_Disable && All_Valid_Tariffs)
            {
                Is_Saving = true;
                NotifyStateChanged();
                var data = await GetAllData(HasSaved);
                data.read_only = true;
                var result = await DATA_ACCESS.PostSerializeQuotation(data);
                if (result is not null && result.Success)
                {
                    HasBeenInitialized = false;
                    HasSaved = true;
                    Global_Disable = true;
                    PDF_Available = true;
                    Shopping_Cart_Hook.FullClear();
                } // ELSE ERROR TODO
                Is_Saving = false;
                NotifyStateChanged();
            }
        }

        public async Task<Stream?> GetQuotationPDF()
        {
            var stream = await DATA_ACCESS.PostDownloadPDF(GetQuotationGUID(), LanguageUtil.getCurrentCulture(), WhiteLabel.Current_Page);
            return stream;
        }

        private async Task RemoveAssociations(int serviceId)
        {
            await Task.Run(() =>
            {
                serviceElement_pairs.Remove(serviceId, out HashSet<int>? element_pairs);
                bool IsNotAssociated = true;
                if (subservices_data.Remove(serviceId, out IDictionary<int, bool>? subservices) && subservices is not null && subservices.Any())
                {
                    foreach (var subserviceID in subservices.Keys.Where(subserviceID => campos_tarifarios.ContainsKey(subserviceID)))
                    {
                        campos_tarifarios.Remove(subserviceID);
                    }
                }
                if (element_pairs is not null)
                {
                    foreach (var pair in element_pairs)
                    {
                        quotation_data.TryGetValue(pair, out ElementQuotationData? element_data);
                        if (element_data is not null)
                        {
                            IsNotAssociated = element_data.GetAssociations().Count <= 1;
                            if (IsNotAssociated)
                            {
                                element_data.Dispose();
                                quotation_data.Remove(pair);
                            }
                            element_data.RemoveAssociation(serviceId);
                            UnsetDataReady(serviceId);
                        }
                    }
                }
            });
        }

        public async Task RemovePackage(int packageId, IEnumerable<int> PackageServices)
        {
            package_ids.Remove(packageId);
            tarifas_paquete.Remove(packageId);
            package_services_ids.Remove(packageId);
            foreach (var serviceId in PackageServices)
            {
                service_data_ready.Remove(serviceId);
                subservices_data.Remove(serviceId);
                packages_reverselookup.Remove(serviceId);
                await RemoveAssociations(serviceId);
            }

            await UpdateTotals();
            NotifyStateChanged();
        }

        public async Task RemovePackageService(int packageId, int serviceId)
        {
            service_data_ready.Remove(serviceId);
            subservices_data.Remove(serviceId);
            packages_reverselookup.Remove(serviceId);
            await RemoveAssociations(serviceId);
        }

        public async Task RemoveService(int serviceId)
        {
            service_data_ready.Remove(serviceId);
            subservices_data.Remove(serviceId);
            tarifas_servicios.Remove(serviceId);
            await RemoveAssociations(serviceId);
            await UpdateTotals();
            NotifyStateChanged();
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

        public IDictionary<int, IDictionary<int, bool>> GetSubservicesData()
        {
            return subservices_data;
        }

        public bool GetSubserviceData(int serviceID, int subserviceID)
        {
            return subservices_data[serviceID][subserviceID];
        }

        // TODO: Update function to utilize the new entities
        public async Task SetSubserviceData(int serviceID, int subserviceID, bool value)
        {
            if (!Global_Disable)
            {
                subservices_data[serviceID][subserviceID] = value;
                // TODO Verify behavior
                //if (!value && tarifas_subservicios.TryGetValue(serviceID, out var subservicios_afecta) && subservicios_afecta.TryGetValue(subserviceID, out _))
                //{
                //    tarifas_subservicios[serviceID].Remove(subserviceID);
                //}
                if (ServiceComplete(serviceID))
                {
                    await CheckServiceDataReady(new List<int>() { serviceID });
                }
            }
        }

        public IDictionary<int, PackageTariff> GetPackagesTariffs()
        {
            return tarifas_paquete;
        }

        public async Task DiscardPackages(IEnumerable<int> packageIDs)
        {
            await Task.Run(() =>
            {
                foreach (var packageID in packageIDs)
                {
                    tarifas_paquete.Remove(packageID);
                }
            });
        }

        public async Task DiscardServices(IEnumerable<int> serviceIDs)
        {
            await Task.Run(() =>
            {
                foreach (var serviceID in serviceIDs)
                {
                    bool IsNotAssociated = true;
                    serviceElement_pairs.Remove(serviceID, out HashSet<int>? element_pairs);
                    tarifas_servicios.Remove(serviceID);
                    campos_tarifarios.Remove(serviceID);
                    UnsetDataReady(serviceID);
                    if (subservices_data.Remove(serviceID, out IDictionary<int, bool>? subservices) && subservices is not null && subservices.Any())
                    {
                        foreach (var subserviceID in subservices.Keys.Where(subserviceID => campos_tarifarios.ContainsKey(subserviceID)))
                        {
                            campos_tarifarios.Remove(subserviceID);
                        }
                    }
                    if (element_pairs is not null)
                    {
                        foreach (var pair in element_pairs)
                        {
                            quotation_data.TryGetValue(pair, out ElementQuotationData? element_data);
                            if (element_data is not null)
                            {
                                IsNotAssociated = element_data.GetAssociations().Count <= 1;
                                if (IsNotAssociated)
                                {
                                    element_data.Dispose();
                                    quotation_data.Remove(pair);
                                }
                                element_data.RemoveAssociation(serviceID);
                            }
                        }
                    }
                }
            });
        }

        private void UnsetDataReady(int serviceID)
        {
            service_data_ready[serviceID] = false;
        }

        public void SetServiceIDs(IEnumerable<int> ids)
        {
            service_ids = ids.ToList();
        }

        public IList<int> GetServiceIds()
        {
            return service_ids;
        }

        public void SetPackageIDs(IEnumerable<int> ids)
        {
            package_ids = ids.ToList();
        }

        public IList<int> GetPackageIds()
        {
            return package_ids;
        }

        public void SetPackagesReverseLookup(IDictionary<int, int> reverse_lookup)
        {
            packages_reverselookup = reverse_lookup;
        }

        public IDictionary<int, int> GetPackagesReverseLookup()
        {
            return packages_reverselookup;
        }

        public void AddNewTarifaFields(int serviceID, IEnumerable<ServicioCamposTarifariosEntidad> fields)
        {
            campos_tarifarios[serviceID] = fields;
        }

        public IDictionary<int, IEnumerable<ServicioCamposTarifariosEntidad>> GetTarifaFields()
        {
            return campos_tarifarios;
        }

        public void AddNewQuotationData(int elementID, ElementQuotationData componentDI)
        {
            quotation_data.Add(elementID, componentDI);
        }

        public HashSet<int> GetElements(IEnumerable<int> service_ids)
        {
            return service_ids.SelectMany(service_id => serviceElement_pairs[service_id]).ToHashSet();
        }

        public IDictionary<int, HashSet<int>> GetServiceElementPairs()
        {
            return serviceElement_pairs;
        }

        public IDictionary<int, ElementQuotationData> GetQuotationData()
        {
            return quotation_data;
        }

        public ElementQuotationData GetElementQuotationData(int elementID)
        {
            return quotation_data[elementID];
        }

        private void AddServiceTariff(int serviceID, ServiceTariff tariff)
        {
            tarifas_servicios[serviceID] = tariff;
        }

        private void AddPackageTariff(int packageID, PackageTariff tariff)
        {
            tarifas_paquete[packageID] = tariff;
        }

        public bool ElementDataExists(int elementID)
        {
            return quotation_data.ContainsKey(elementID);
        }

        public bool ServiceDataExists(IEnumerable<int> service_ids)
        {
            return service_ids.All(service_id => serviceElement_pairs.ContainsKey(service_id));
        }

        public bool ServiceDataExists(int serviceID)
        {
            return serviceElement_pairs.ContainsKey(serviceID);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceIDs"></param>
        /// <returns></returns>
        public async Task CheckServiceDataReady(IEnumerable<int> serviceIDs)
        {
            bool any_changes_aggregate = false;

            foreach (var serviceID in serviceIDs)
            {
                bool result;
                if (serviceElement_pairs.TryGetValue(serviceID, out var fields))
                {
                    var result_inner = fields.Select(key => { ElementQuotationData value; quotation_data.TryGetValue(key, out value!); return value; }).Where(selection => selection != null);
                    result = result_inner.All(data => data.AllFieldsAnswered());
                }
                else
                {
                    result = false;
                }

                service_data_ready[serviceID] = result;

                var any_changes = await TriggerCalculation(serviceID, result);
                any_changes_aggregate = any_changes_aggregate || any_changes;
            }

            if (any_changes_aggregate)
            {
                await SaveQuotation();
                NotifyStateChanged();
            }
        }

        private bool ShouldTriggerPackage(int package_id)
        {
            return package_services_ids[package_id].All(service_id => service_data_ready[service_id]);
        }

        public async Task<bool> TriggerCalculation(int serviceID, bool data_ready)
        {
            bool any_changes = false;
            if (data_ready)
            {
                if (!Is_Calculating)
                {
                    Is_Calculating = true;
                    NotifyStateChanged();
                }

                // Service comes from a package
                if (packages_reverselookup.TryGetValue(serviceID, out int package_id))
                {
                    // Trigger package calculation
                    if (ShouldTriggerPackage(package_id))
                    {
                        await CalcularTarifasPaquete(package_id);
                        any_changes = true;
                    }
                    // TODO FIGURE OUT IF AN ELSE SHOULD GO HERE
                }
                // Single service
                else
                {
                    await CalcularTarifasServicio(serviceID);
                    any_changes = true;
                }
            }
            // Remover tarifa si los datos no están completos y si existe una tarifa
            else
            {
                // Service comes from a package
                if (packages_reverselookup.TryGetValue(serviceID, out int package_id))
                {
                    tarifas_paquete.Remove(package_id);
                }
                else if (tarifas_servicios.TryGetValue(serviceID, out _))
                {
                    any_changes = true;
                    tarifas_servicios.Remove(serviceID);
                }
            }
            Is_Calculating = false;

            return any_changes;
        }

        public async Task CalcularTarifasPaquete(int package_id)
        {
            ClientQuotationData data = new() { Client_ID = Client_ID, Money_Kind = money_kind, Service_Data = new Dictionary<int, ServiceQuotationData>(), Package_ID = package_id };

            foreach (var package_service_id in package_services_ids[package_id])
            {
                // Datos del servicio
                var fields_values = await Task.Run(() => GetFieldsAndGuidValues(package_service_id));
                var service_values = GetAllValues(package_service_id);
                var service_version = Static_Shopping_Cart.GetPackages()[package_id].Servicios[package_service_id].Version;

                // TODO Proper async
                // Datos para subservicios
                var subservices_to_quote = await Task.Run(() => GetSubservices(package_service_id));

                if (subservices_to_quote.Any())
                {
                    IList<SubserviceQuotationData> subservice_data = new List<SubserviceQuotationData>();
                    foreach (var subservice_kvp in subservices_to_quote)
                    {
                        var subservice_fields_values = await Task.Run(() => GetFieldsAndGuidValues(subservice_kvp.Item1));
                        var subservice_values = GetAllValues(subservice_kvp.Item1);
                        var subservice_version = Static_Shopping_Cart.GetPackages()[package_id].Servicios[package_service_id].Subservicios[subservice_kvp.Item1].Version;
                        subservice_data.Add(new SubserviceQuotationData() { Subservice_ID = subservice_kvp.Item1, Effects_Tariff = subservice_kvp.Item2, Fields_Values = new FieldsValuesWrapper(subservice_fields_values), Service_Values = subservice_values, Version = subservice_version });
                    }
                    data.Service_Data[package_service_id] = new ServiceQuotationData() { Fields_Values = new FieldsValuesWrapper(fields_values), Service_Values = service_values, Subservice_Data = subservice_data, Version = service_version };
                }
                else
                {
                    data.Service_Data[package_service_id] = new ServiceQuotationData() { Fields_Values = new FieldsValuesWrapper(fields_values), Service_Values = service_values, Subservice_Data = null, Version = service_version };
                }
            }


            // Post data to server and obtain a tariff calculation result
            TariffCalculationResult result = await DATA_ACCESS.GetCalculatedTariff(data);

            AddPackageTariff(package_id, result.Tarifa_Paquete);

            ReCalculateAllValidTariffs();

            await UpdateTotals();
        }

        private async Task CalcularTarifasServicio(int serviceID)
        {
            ClientQuotationData data = new() { Client_ID = Client_ID, Money_Kind = money_kind, Service_Data = new Dictionary<int, ServiceQuotationData>(), Package_ID = null };
            // Datos del servicio
            var fields_values = await Task.Run(() => GetFieldsAndGuidValues(serviceID));
            var service_values = GetAllValues(serviceID);
            var service_version = Static_Shopping_Cart.GetServices()[serviceID].Version;

            // TODO Proper async
            // Datos para subservicios
            var subservices_to_quote = await Task.Run(() => GetSubservices(serviceID));

            if (subservices_to_quote.Any())
            {
                IList<SubserviceQuotationData> subservice_data = new List<SubserviceQuotationData>();
                foreach (var subservice_kvp in subservices_to_quote)
                {
                    var subservice_fields_values = await Task.Run(() => GetFieldsAndGuidValues(subservice_kvp.Item1));
                    var subservice_values = GetAllValues(subservice_kvp.Item1);
                    var subservice_version = Static_Shopping_Cart.GetServices()[serviceID].Subservicios[subservice_kvp.Item1].Version;
                    subservice_data.Add(new SubserviceQuotationData() { Subservice_ID = subservice_kvp.Item1, Effects_Tariff = subservice_kvp.Item2, Fields_Values = new FieldsValuesWrapper(subservice_fields_values), Service_Values = subservice_values, Version = subservice_version });
                }
                data.Service_Data[serviceID] = new ServiceQuotationData() { Fields_Values = new FieldsValuesWrapper(fields_values), Service_Values = service_values, Subservice_Data = subservice_data, Version = service_version };
            }
            else
            {
                data.Service_Data[serviceID] = new ServiceQuotationData() { Fields_Values = new FieldsValuesWrapper(fields_values), Service_Values = service_values, Subservice_Data = null, Version = service_version };
            }

            // Post data to server and obtain a tariff calculation result
            TariffCalculationResult result = await DATA_ACCESS.GetCalculatedTariff(data);

            AddServiceTariff(serviceID, result.Tarifa_Servicio);

            ReCalculateAllValidTariffs();

            await UpdateTotals();
        }

        private async Task<EMoney> GetMoneyKind()
        {
            // OPTION 1 = USD, OPTION 2 = MXN
            string option = await DATA_ACCESS.GetParametro(5);
            var int_option = (EMoney)Convert.ToInt32(option);
            return int_option;
        }

        private async Task<decimal> GetCoinsRate(EMoney money_kind)
        {
            if (money_kind == EMoney.USD)
            {
                string rate = await DATA_ACCESS.GetParametro(10);
                return Convert.ToDecimal(rate);
            }
            else
            {
                string rate = await DATA_ACCESS.GetParametro(11);
                return Convert.ToDecimal(rate);
            }
        }

        /// <summary>
        /// Obtiene todos los valores numéricos asociados a un servicio o subservicio
        /// </summary>
        /// <param name="serviceID">ID del servicio o subservicio del cual se quieren obtener todos sus valores numéricos</param>
        /// <returns>Diccionario con el ID del campo y su valor correspondiente</returns>
        private IDictionary<int, object> GetAllValues(int serviceID)
        {
            if (serviceElement_pairs.TryGetValue(serviceID, out var all_fields))
            {
                return all_fields.SelectMany(key => { ElementQuotationData value; quotation_data.TryGetValue(key, out value!); return value.GetAllNumericData(); }).ToDictionary(pair => pair.Key, pair => pair.Value);
            }
            else
            {
                return new Dictionary<int, object>();
            }
        }

        /// <summary>
        /// Obtiene todos los IDs de los campos de todos los elementos vinculados con un servicio o subservicio
        /// Y sus valores correspondientes
        /// </summary>
        /// <param name="serviceID">ID del servicio o subservicio del cual se quieren obtener sus campos y valores</param>
        /// <returns>Tuple que contiene una lista de los IDs de los campos de los elementos del servicio o subservicio y una lista de sus valores correspondientes</returns>
        private Tuple<IEnumerable<string>, IEnumerable<Guid>> GetFieldsAndGuidValues(int serviceID)
        {
            var fields = new List<string>();
            var values = new List<Guid>();
            if (campos_tarifarios.TryGetValue(serviceID, out IEnumerable<ServicioCamposTarifariosEntidad>? value))
            {
                foreach (var field in value)
                {
                    fields.Add(field.CamposTarifarios);
                    foreach (var item in field.CamposTarifarios.Split(','))
                    {
                        values.Add(quotation_data[field.ID].GetGuidFields()![Convert.ToInt32(item)]!.Value);
                    }
                }
            }
            return new Tuple<IEnumerable<string>, IEnumerable<Guid>>(fields, values);
        }

        /// <summary>
        /// Obtiene los IDs de los subservicios de un servicio en específico y si estos afectan o no la tarifa
        /// </summary>
        /// <param name="serviceID">ID del servicio del cual se quieren obtener sus subservicios</param>
        /// <returns>Diccionario de IDs de subservicios y si estos afectan o no la tarifa</returns>
        public IEnumerable<Tuple<int, bool>> GetSubservices(int serviceID)
        {
            if (subservices_data.TryGetValue(serviceID, out IDictionary<int, bool>? subservices) && subservices is not null && subservices.Any())
            {
                IEnumerable<int> subservicesIds = subservices.Where(keyvalue => keyvalue.Value).Select(keyvalue => keyvalue.Key);
                IEnumerable<Tuple<int, bool>> subservices_to_quote = subservicesIds.Select(key => { SubserviciosEntidad value; Static_Shopping_Cart.GetServices()[serviceID].Subservicios.TryGetValue(key, out value!); return value; }).Select(subservicio => { return new Tuple<int, bool>(subservicio.Id_Subservicio, subservicio.Afecta_Tarifa); });
                return subservices_to_quote;
            }
            else
            {
                return new List<Tuple<int, bool>>();
            }
        }

        private async Task UpdateTotals()
        {
            Totals.Subtotal = GetSubtotal();
            Totals.Packages_Discount = GetPackageDiscounts();
            Totals.Commercial_Discount = GetComDiscounts();
            Totals.Total_Taxes = GetTaxTotals();
            Totals.Total = GetTotal();
            if (Pay_CIMA_Coins)
            {
                await ComputeCoinsDiscount();
            }
            else
            {
                Totals.Total_To_Pay = Totals.Total;
            }
            Totals.CIMA_Coins_Won = await GetCimaCoins();
        }

        private decimal GetTaxTotals()
        {
            decimal packages_subtotal = 0;
            decimal services_subtotal = 0;

            if (GetPackagesTariffs().Any())
            {
                foreach (var item in GetPackagesTariffs().Where(tariff => tariff.Value.Valid_Tariff))
                {
                    if (item.Value.Tarifa_Base.Tarifas_Servicios_Spread is not null)
                    {
                        var package_total = item.Value.Tarifa_Base.Tarifas_Servicios_Spread.Values?.Where(tariff => tariff.Tax_Total.HasValue).Sum(tariff => tariff.Tax_Total!.Value) ?? 0;
                        var package_subservices_total = item.Value.Tarifa_Base.Tarifas_Servicios_Spread.Values!.SelectMany(tariff => tariff.Subservices_Tax_Totals ?? Enumerable.Empty<KeyValuePair<int, decimal?>>())
                                                        .Where(kvp => kvp.Value.HasValue).Sum(value => value.Value!.Value);
                        packages_subtotal += package_total + package_subservices_total;
                    }
                    else if (item.Value.Tarifa_Base_Minima is not null)
                    {
                        var package_total = item.Value.Tarifa_Base_Minima.Tarifas_Servicios.Values?.Where(tariff => tariff.Tax_Total.HasValue).Sum(tariff => tariff.Tax_Total!.Value) ?? 0;
                        var package_subservices_total = item.Value.Tarifa_Base_Minima.Tarifas_Servicios.Values!.SelectMany(tariff => tariff.Subservices_Tax_Totals ?? Enumerable.Empty<KeyValuePair<int, decimal?>>())
                                                        .Where(kvp => kvp.Value.HasValue).Sum(value => value.Value!.Value);
                        packages_subtotal += package_total + package_subservices_total;
                    }
                    else
                    {
                        var package_total = item.Value.Tarifa_Base.Tarifas_Servicios.Values?.Where(tariff => tariff.Tax_Total.HasValue).Sum(tariff => tariff.Tax_Total!.Value) ?? 0;
                        var package_subservices_total = item.Value.Tarifa_Base.Tarifas_Servicios.Values!.SelectMany(tariff => tariff.Subservices_Tax_Totals ?? Enumerable.Empty<KeyValuePair<int, decimal?>>())
                                                        .Where(kvp => kvp.Value.HasValue).Sum(value => value.Value!.Value);
                        packages_subtotal += package_total + package_subservices_total;
                    }
                }
            }

            if (GetServiceTariffs().Any())
            {
                foreach (var item in GetServiceTariffs().Where(tariff => tariff.Value.Valid_Tariff))
                {
                    services_subtotal += item.Value.Tax_Total ?? 0;
                    // Subservices_Tax_Totals IS NULL
                    services_subtotal += item.Value.Subservices_Tax_Totals?.Where(kvp => kvp.Value.HasValue).Sum(value => value.Value!.Value) ?? 0;
                }
            }

            return packages_subtotal + services_subtotal;
        }

        private async Task ComputeCoinsDiscount()
        {
            var user_context = await ((ApiAuthenticationStateProvider)AuthProvider).GetAuthenticationStateAsync();
            var coins = Convert.ToInt32(user_context.User.Claims.FirstOrDefault(claims => claims.Type == "CIMA Coins")!.Value);
            var max_discount = coins * Coins_Rate;
            if (max_discount > Totals.Total)
            {
                Totals.CIMA_Coins_Discount = Totals.Total;
                Totals.Spent_CIMA_Coins = Convert.ToInt32(Math.Ceiling(Totals.Total / Coins_Rate));
                Totals.Total_To_Pay = 0;
            }
            else
            {
                if (max_discount == Totals.Total)
                {
                    Totals.CIMA_Coins_Discount = Totals.Total;
                    Totals.Spent_CIMA_Coins = coins;
                    Totals.Total_To_Pay = 0;
                }
                else
                {
                    Totals.CIMA_Coins_Discount = max_discount!;
                    Totals.Spent_CIMA_Coins = coins;
                    Totals.Total_To_Pay = Totals.Total - max_discount!;
                }
            }
        }

        private async Task<int> GetCimaCoins()
        {
            string coins_multiplier;
            if (money_kind == EMoney.USD)
            {
                coins_multiplier = await DATA_ACCESS.GetParametro(7);
            }
            else
            {
                coins_multiplier = await DATA_ACCESS.GetParametro(8);
            }

            return Convert.ToInt32(Math.Round((decimal)(Convert.ToInt32(Math.Floor(Convert.ToDecimal(coins_multiplier) * Totals.Total_To_Pay)) / 10)) * 10);
        }

        private decimal GetSubtotal()
        {
            decimal packages_subtotal = 0;
            decimal services_subtotal = 0;

            if (GetPackagesTariffs().Any())
            {
                foreach (var item in GetPackagesTariffs().Where(tariff => tariff.Value.Valid_Tariff))
                {
                    packages_subtotal += item.Value.Tarifa_Base.Tarifa_Subtotal;
                }
            }

            if (GetServiceTariffs().Any())
            {
                foreach (var item in GetServiceTariffs().Where(tariff => tariff.Value.Valid_Tariff))
                {
                    services_subtotal += item.Value.Service_Tariff.Tarifa_Subtotal;
                }
            }

            return packages_subtotal + services_subtotal;
        }

        private decimal GetAllTotals()
        {
            decimal packages_subtotal = 0;
            decimal services_subtotal = 0;

            if (GetPackagesTariffs().Any())
            {
                foreach (var item in GetPackagesTariffs().Where(tariff => tariff.Value.Valid_Tariff))
                {
                    packages_subtotal += item.Value.Tarifa_Base.Tarifa_Total_No_Tax;
                }
            }

            if (GetServiceTariffs().Any())
            {
                foreach (var item in GetServiceTariffs().Where(tariff => tariff.Value.Valid_Tariff))
                {
                    services_subtotal += item.Value.Service_Tariff.Tarifa_Total_No_Tax;
                }
            }

            return packages_subtotal + services_subtotal;
        }

        private decimal? GetPackageDiscounts()
        {
            decimal _packages_discounts = 0;
            if (GetPackagesTariffs().Any())
            {
                foreach (var package in GetPackagesTariffs().Values.Where(package => package.Valid_Tariff && package.Tarifa_Preferencial is not null && package.Tarifa_Comercial is null))
                {
                    _packages_discounts += package.Tarifa_Preferencial.Descuento_Total_Calculado;
                }
                foreach (var package in GetPackagesTariffs().Values.Where(package => package.Valid_Tariff && package.Tarifa_Base_Minima is not null && package.Tarifa_Comercial is null && package.Tarifa_Preferencial is null))
                {
                    _packages_discounts += package.Tarifa_Base_Minima.Descuento_Total_Calculado;
                }
                return _packages_discounts;
            }
            else
            {
                return null;
            }
        }

        private decimal? GetComDiscounts()
        {
            decimal _com_discounts = 0;
            if (GetPackagesTariffs().Any())
            {
                foreach (var item in GetPackagesTariffs().Values.Where(tariff => tariff.Valid_Tariff && tariff.Tarifa_Comercial is not null))
                {
                    _com_discounts += item.Tarifa_Comercial.Descuento_Base_Total_Calculado;
                }
            }

            if (GetServiceTariffs().Any())
            {
                foreach (var item in GetServiceTariffs().Values.Where(tariff => tariff.Valid_Tariff && tariff.Service_Tariff.Tarifa_Comercial is not null))
                {
                    _com_discounts += item.Service_Tariff.Tarifa_Comercial.Descuento_Total_Calculado;
                }
            }
            if (_com_discounts == 0)
            {
                return null;
            }
            else
            {
                return _com_discounts;
            }
        }

        private decimal GetTotal()
        {
            var _total = GetAllTotals();
            if (Totals.Packages_Discount is not null && Totals.Packages_Discount != 0)
            {
                _total -= Totals.Packages_Discount.Value;
            }
            if (Totals.Commercial_Discount is not null && Totals.Commercial_Discount != 0)
            {
                _total -= Totals.Commercial_Discount.Value;
            }
            if(Totals.Total_Taxes is not null && Totals.Total_Taxes != 0)
            {
                _total += Totals.Total_Taxes.Value;
            }
            return _total;
        }
    }
}
