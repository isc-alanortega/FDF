using MudBlazor;
using PCG_ENTITIES.Enums;
using PCG_ENTITIES.Extensions;
using PCG_ENTITIES.PCG_FDF;
using PCG_ENTITIES.PCG_FDF.ContractSettigs;
using PCG_ENTITIES.PCG_FDF.PortCapital;
using PCG_ENTITIES.PCG_FDF.QuotationEntities;
using PCG_ENTITIES.PCG_FDF.UtilityEntities;
using PCG_ENTITIES.PCG_FDF.UtilityEntities.Tariffs;
using PCG_ENTITIES.Requests;
using PCG_ENTITIES.Resultsets;
using PCG_FDF.Data.DataAccess;
using PCG_FDF.Data.Entities;
using PCG_FDF.Utility;

namespace PCG_FDF.Data.ComponentDI.Quotation
{
    public class QuotationDataCollection : IQuotation
    {
        /// <summary>
        /// Endpoint asociado a la cotización, determina la Unidad de Negocio
        /// </summary>
        public EPages Endpoint { get; set; }
        /// <summary>
        /// Indica el estado de la cotización
        /// </summary>
        public EStatusCotizacion State { get; set; } = EStatusCotizacion.INITIAL_DATA;
        /// <summary>
        /// Identificador único de la cotización
        /// </summary>
        public Guid UUID { get; set; }
        /// <summary>
        /// ID de la localidad de la cotización
        /// </summary>
        public int Location_ID { get; set; }
        /// <summary>
        /// Indica si la cotización se debe bloquear
        /// </summary>
        public bool Block_Quotation { get; set; }
        /// <summary>
        /// Número de folio, se asigna al guardar de manera final
        /// </summary>
        public string? Invoice { get; set; }
        /// <summary>
        /// ID del usuario que cotiza
        /// </summary>
        public int User_ID { get; set; }
        /// <summary>
        /// ID del cliente que cotiza
        /// </summary>
        public int Client_ID { get; set; }
        /// <summary>
        /// Moneda en la que se está cotizando
        /// </summary>
        public EMoney Money_Kind { get; set; } = EMoney.MXN;
        /// <summary>
        /// Indica si ha sido inicializada
        /// </summary>
        private bool Has_Been_Initialized { get; set; } = false;
        /// <summary>
        /// Indica si ha guardado una vez
        /// </summary>
        private bool Has_Saved { get; set; } = false;
        /// <summary>
        /// Indica si se encuentra calculando
        /// </summary>
        private bool Is_Calculating { get; set; } = false;
        /// <summary>
        /// Indica la URL de la cotización
        /// </summary>
        private string Quotation_URI { get; set; }
        /// <summary>
        /// Indica si la cotización se encuentra guardando
        /// </summary>
        private bool Is_Saving { get; set; } = false;
        /// <summary>
        /// Indica el estado global del formulario de la cotización
        /// </summary>
        private bool Global_Disable { get; set; } = false;
        /// <summary>
        /// Indica si es posible descargar el PDF de esta cotización
        /// </summary>
        private bool PDF_Available { get; set; } = false;
        /// <summary>
        /// Indica si todas las tarifas obtenidas son válidas
        /// </summary>
        private bool All_Valid_Tariffs { get; set; } = false;
        /// <summary>
        /// Carrito estático de la cotización
        /// </summary>
        private StaticShoppingCart Quotation_Cart { get; set; }
        /// <summary>
        /// Objeto que almacena los totales de la cotización
        /// </summary>
        private QTotals Totals { get; set; } = new();
        /// <summary>
        /// Diccionario que guarda los valores de los elementos por card
        /// </summary>
        private IDictionary<int, IDictionary<int, object?>> Shared_Element_Data_Storage { get; set; } = new Dictionary<int, IDictionary<int, object?>>();
        /// <summary>
        /// Diccionario que guarda los valores de elementos de tipo OptionBox
        /// </summary>
        private IDictionary<int, IDictionary<int, MudChip?>> Shared_Chip_Data_Storage { get; set; } = new Dictionary<int, IDictionary<int, MudChip?>>();
        /// <summary>
        /// Diccionario de sets que relaciona un servicio con sus cards
        /// </summary>
        private IDictionary<int, HashSet<int>> Services_Rel_Cards { get; set; } = new Dictionary<int, HashSet<int>>();
        /// <summary>
        /// Diccionario de sets que relaciona un paquete con sus cards
        /// </summary>
        private IDictionary<int, HashSet<int>> Packages_Rel_Cards { get; set; } = new Dictionary<int, HashSet<int>>();
        /// <summary>
        /// Almacena la relación de un servicio con sus posibles subservicios y su estado
        /// </summary>
        private IDictionary<int, IDictionary<int, StatefulSubservice>> Subservices_Data { get; set; } = new Dictionary<int, IDictionary<int, StatefulSubservice>>();
        /// <summary>
        /// Diccionario de cards de la cotización
        /// </summary>
        private IDictionary<int, StatefulCard> Cards { get; set; }
        /// <summary>
        /// Guarda el estado de cada servicio, si los datos requeridos han sido llenados
        /// </summary>
        private IDictionary<int, bool> Service_Data_Ready { get; set; } = new Dictionary<int, bool>();
        /// <summary>
        /// Diccionario de relación de Servicio con su par Clave (ID card) Valor (Campos)
        /// de campos tarifarios
        /// </summary>
        private IDictionary<int, List<KeyValue>> Service_Tariff_Fields { get; set; }
        /// <summary>
        /// Servicios de la cotización, con su par ID, Versión
        /// </summary>
        private IDictionary<int, KeyValueInt> Quotation_Services { get; set; } = new Dictionary<int, KeyValueInt>();
        /// <summary>
        /// Paquetes de la cotización y sus servicios con su par ID, Versión
        /// </summary>
        private IDictionary<int, IEnumerable<KeyValueInt>> Quotation_Packages_Services { get; set; } = new Dictionary<int, IEnumerable<KeyValueInt>>();
        /// <summary>
        /// Diccionario inverso que relaciona un servicio con el paquete al que pertenece
        /// </summary>
        private IDictionary<int, int> Quotation_Packages_Services_Reverse { get; set; }
        /// <summary>
        /// Diccionario de tarifas de servicios
        /// </summary>
        private IDictionary<int, ServiceTariff> Services_Tariffs { get; set; } = new Dictionary<int, ServiceTariff>();
        /// <summary>
        /// Diccionario de tarifas de paquetes
        /// </summary>
        private IDictionary<int, PackageTariff> Packages_Tariffs { get; set; } = new Dictionary<int, PackageTariff>();

        #region PortCapital Variables
        /// <summary>
        /// Bandera que nos indica que la cotizacion se puede financiar por PortCapital
        /// </summary>
        private bool Is_PortCapital { get; set; } = false;
        /// <summary>
        /// Bandera para indicar que esta listo para cotizar por PortCapital
        /// </summary>
        private bool ISReadyQuotationPortCapital { get; set; } = false;
        /// <summary>
        /// Bandera para indicar que ya se encuentra con datos la cotizacion p
        /// </summary>
        private bool HasDataPortCapital { get; set; } = false;
        /// <summary>
        /// Data Quotation PortCapital
        /// </summary>
        private PortCapitalResponse<PortCapitalQuotationResponse>? PortCapitalObj { get; set; } = null;
        #endregion

        /// <summary>
        /// Action that triggers a StateHasChanged event to rerender the component
        /// </summary>
        public event Action OnChange;

        private readonly PCG_FDF_DB _dataAccessService;
        private readonly ShoppingCartContainer _shoppingCartHook;

        public QuotationDataCollection(PCG_FDF_DB dataAccessService, ShoppingCartContainer shoppingCartHook)
        {
            _dataAccessService = dataAccessService;
            _shoppingCartHook = shoppingCartHook;
        }

        /// <summary>
        /// Dispara el evento de StateHasChanged y notifica al componente principal
        /// </summary>
        private void NotifyStateChanged() => OnChange?.Invoke();

        public void CreateQuotationCart(ShoppingCartContainer cart) => Quotation_Cart = new StaticShoppingCart(cart);

        public bool ShowContract() => State != EStatusCotizacion.CONTRACTED;

        public void ChangeTypeMoneyKind(EMoney money) => Money_Kind = money;

        public void ResetSelf()
        {
            Has_Been_Initialized = false;
            Has_Saved = false;
            State = EStatusCotizacion.INITIAL_DATA;
            Is_Calculating = false;
            Is_Saving = false;
            Global_Disable = false;
            PDF_Available = false;
            Totals = new QTotals();
            UUID = Guid.Empty;
            Invoice = null;
            All_Valid_Tariffs = false;
            Shared_Element_Data_Storage.Clear();
            Shared_Chip_Data_Storage.Clear();
            Services_Rel_Cards.Clear();
            Packages_Rel_Cards.Clear();
            Subservices_Data.Clear();
            Cards?.Clear();
            Service_Data_Ready.Clear();
            Service_Tariff_Fields?.Clear();
            Quotation_Services.Clear();
            Quotation_Packages_Services.Clear();
            Quotation_Packages_Services_Reverse?.Clear();
            Services_Tariffs.Clear();
            Packages_Tariffs.Clear();

            //return;
            ISReadyQuotationPortCapital = false;
            Is_PortCapital = false;
            HasDataPortCapital = false;
            PortCapitalObj = null;
        }

        public void Initialize(QuotationInitializer source, string webpage, IEnumerable<KeyValueInt> services, IDictionary<int, IEnumerable<KeyValueInt>> package_services)
        {
            try
            {
                Has_Been_Initialized = false;

                UUID = source.UUID;
                State = EStatusCotizacion.INITIAL_DATA;
                Endpoint = source.Endpoint;
                Quotation_URI = webpage;
                Location_ID = source.Location_ID;
                User_ID = source.User_ID;
                Client_ID = source.Client_ID;
                Money_Kind = source.Money_Kind;
                Services_Rel_Cards = source.Services_Rel_Cards;
                Packages_Rel_Cards = source.Packages_Rel_Cards;
                Cards = source.Cards.ToDictionary(key => key.Key, value => new StatefulCard(value.Value));
                Service_Tariff_Fields = source.Service_Tariff_Fields;
                Quotation_Services = services.ToDictionary(key => key.Key, value => value);
                Quotation_Packages_Services = package_services;
                Quotation_Packages_Services_Reverse = new Dictionary<int, int>();

                foreach (var package_services_kvp in package_services)
                {
                    foreach (var service in package_services_kvp.Value)
                    {
                        Quotation_Packages_Services_Reverse[service.Key] = package_services_kvp.Key;
                    }
                }

                foreach (var card in Cards.Values)
                {
                    Shared_Element_Data_Storage[card.Card_ID] = new Dictionary<int, object?>();
                    foreach (var element in card.Elements.Values)
                    {
                        Shared_Element_Data_Storage[card.Card_ID][element.Element_ID] = null;
                    }
                }

                Subservices_Data = Quotation_Cart.GetSubservicesPairs();

                Has_Been_Initialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Initialize(QuotationInitializer source, string webpage, MiddlewareShoppingCart cart_data)
        {
            Has_Been_Initialized = false;

            UUID = source.UUID;
            Endpoint = source.Endpoint;
            Quotation_URI = webpage;
            Location_ID = source.Location_ID;
            State = EStatusCotizacion.INITIAL_DATA;
            User_ID = source.User_ID;
            Client_ID = source.Client_ID;
            Money_Kind = source.Money_Kind;
            Services_Rel_Cards = source.Services_Rel_Cards;
            Packages_Rel_Cards = source.Packages_Rel_Cards;
            Quotation_Cart = new StaticShoppingCart(cart_data);
            Cards = source.Cards.ToDictionary(key => key.Key, value => new StatefulCard(value.Value));
            Service_Tariff_Fields = source.Service_Tariff_Fields;
            Quotation_Services = Quotation_Cart.GetServiceVersionPairs().ToDictionary(key => key.Key, value => value);
            Quotation_Packages_Services = Quotation_Cart.GetPackageServiceVersionPairs();
            Quotation_Packages_Services_Reverse = new Dictionary<int, int>();

            foreach (var package_services_kvp in Quotation_Packages_Services)
            {
                foreach (var service in package_services_kvp.Value)
                {
                    Quotation_Packages_Services_Reverse[service.Key] = package_services_kvp.Key;
                }
            }

            foreach (var card in Cards.Values)
            {
                Shared_Element_Data_Storage[card.Card_ID] = new Dictionary<int, object?>();
                foreach (var element in card.Elements.Values)
                {
                    Shared_Element_Data_Storage[card.Card_ID][element.Element_ID] = null;
                }
            }

            Subservices_Data = Quotation_Cart.GetSubservicesPairs();

            Has_Been_Initialized = true;
        }

        public void Initialize(QuotationMiddleware source, MiddlewareShoppingCart cart_data, EStatusCotizacion State)
        {
            Has_Been_Initialized = false;

            Endpoint = source.Endpoint;
            UUID = source.UUID;
            Invoice = source.Invoice;
            this.State = State;
            Location_ID = source.Location_ID;
            User_ID = source.User_ID;
            Client_ID = source.Client_ID;
            Money_Kind = source.Money_Kind;
            Has_Saved = source.Has_Saved;
            Is_Calculating = false;
            Quotation_URI = source.Quotation_URI;
            Is_Saving = false;
            Global_Disable = source.Definitive;
            PDF_Available = source.Definitive;
            All_Valid_Tariffs = source.All_Valid_Tariffs;
            Quotation_Cart = new StaticShoppingCart(cart_data);
            Totals = source.Totals;
            Shared_Element_Data_Storage = source.Shared_Element_Data_Storage;
            Services_Rel_Cards = source.Services_Rel_Cards;
            Packages_Rel_Cards = source.Packages_Rel_Cards;
            Subservices_Data = source.Subservices_Data.ToDictionary(key => key.Key, value => (IDictionary<int, StatefulSubservice>)value.Value.ToDictionary(key => key.Key, value => new StatefulSubservice(value.Value)));
            Cards = source.Cards.ToDictionary(key => key.Key, value => new StatefulCard(value.Value));
            Service_Data_Ready = source.Service_Data_Ready;
            Service_Tariff_Fields = source.Service_Tariff_Fields;
            Quotation_Services = source.Quotation_Services;
            Quotation_Packages_Services = source.Quotation_Packages_Services;
            Quotation_Packages_Services_Reverse = source.Quotation_Packages_Services_Reverse;
            Services_Tariffs = source.Services_Tariffs;
            Packages_Tariffs = source.Packages_Tariffs;
            Money_Kind = source.Money_Kind;

            Has_Been_Initialized = true;
        }

        public StaticShoppingCart GetQuotationCart() => Quotation_Cart;

        public (string Name, string ViewBox, string Icon) GetProductData(int? serviceId, int? packageId, bool isPackage, bool isPackageService)
        {
            if (isPackage)
            {
                return (Quotation_Cart.GetPackages()[packageId!.Value].Paquete.Nombre_Paquete,
                    Quotation_Cart.GetPackages()[packageId!.Value].Paquete.Viewbox,
                    Quotation_Cart.GetPackages()[packageId!.Value].Paquete.Icon);
            }

            if (isPackageService)
            {
                return (Quotation_Cart.GetPackages()[packageId!.Value].Servicios[serviceId!.Value].FullName,
                    Quotation_Cart.GetPackages()[packageId!.Value].Servicios[serviceId!.Value].ViewBox,
                    Quotation_Cart.GetPackages()[packageId!.Value].Servicios[serviceId!.Value].Icon
                    );
            }

            return (Quotation_Cart.GetServices()[serviceId!.Value].FullName,
                Quotation_Cart.GetServices()[serviceId!.Value].ViewBox,
                Quotation_Cart.GetServices()[serviceId!.Value].Icon
                );
        }

        private void RemoveAssociations(int serviceId)
        {
            if (Services_Rel_Cards is not null && Service_Tariff_Fields is not null)
            {
                Services_Rel_Cards.Remove(serviceId, out HashSet<int>? card_ids);
                Service_Tariff_Fields.Remove(serviceId);
                bool IsNotAssociated = true;

                if (Subservices_Data.Remove(serviceId, out IDictionary<int, StatefulSubservice>? subservices) && subservices is not null && subservices.Any())
                {
                    foreach (var subserviceID in subservices.Keys.Where(subserviceID => Service_Tariff_Fields.ContainsKey(subserviceID)))
                    {
                        Service_Tariff_Fields.Remove(subserviceID);
                    }
                }

                if (card_ids is not null)
                {
                    foreach (var card in card_ids)
                    {
                        IsNotAssociated = Cards[card].From_Services.Count <= 1;
                        Cards[card].RemoveAssociation(serviceId);
                        if (IsNotAssociated)
                        {
                            Cards.Remove(card);
                        }
                    }
                }
            }
        }

        public void RemovePackage(int packageId)
        {
            IEnumerable<KeyValueInt>? PackageServices = null;
            Quotation_Packages_Services?.Remove(packageId, out PackageServices);
            Packages_Tariffs?.Remove(packageId);

            if (PackageServices is not null)
            {
                foreach (var service_kvp in PackageServices.Select(service => service.Key))
                {
                    Service_Data_Ready?.Remove(service_kvp);
                    Subservices_Data?.Remove(service_kvp);
                    Quotation_Packages_Services_Reverse?.Remove(service_kvp);
                    RemoveAssociations(service_kvp);
                }
            }

            UpdateTotals();
            NotifyStateChanged();
        }

        public void RemovePackageService(int serviceId)
        {
            Service_Data_Ready?.Remove(serviceId);
            Subservices_Data?.Remove(serviceId);
            Quotation_Packages_Services_Reverse?.Remove(serviceId);
            RemoveAssociations(serviceId);
        }

        public void RemoveService(int serviceId)
        {
            Service_Data_Ready.Remove(serviceId);
            Services_Tariffs.Remove(serviceId);
            RemoveAssociations(serviceId);
            Subservices_Data.Remove(serviceId);
            UpdateTotals();
            NotifyStateChanged();
        }

        public bool GetQuotationHasSaved() => Has_Saved;

        public string GetQuotationURI() => Quotation_URI;

        public bool GetAllValidTariffs() => All_Valid_Tariffs;

        public QTotals GetTotals() => Totals;

        public IDictionary<int, IDictionary<int, object?>> GetStorage() => Shared_Element_Data_Storage;

        public IDictionary<int, HashSet<int>> GetServicesCards() => Services_Rel_Cards;

        public IDictionary<int, HashSet<int>> GetPackagesCards() => Packages_Rel_Cards;

        public bool HasCommercial()
        {
            var comPackages = Packages_Tariffs.Values.Any(package => package.Tarifa_Comercial is not null);
            var comServices = Services_Tariffs.Values.Any(service => service.Service_Tariff is not null && service.Service_Tariff.Tarifa_Comercial is not null);
            return comPackages || comServices;
        }

        public (bool Any, IDictionary<int, StatefulSubservice>? Subservices) TryGetSubservices(int? serviceId)
        {
            if (serviceId.HasValue && Subservices_Data.TryGetValue(serviceId.Value!, out var subservices))
            {
                return (true, subservices);
            }
            return (false, null);
        }

        public async Task SetSubserviceStatus(bool value, int serviceId, int Subservice_ID)
        {
            Subservices_Data[serviceId][Subservice_ID].Selected = value;
            if (Service_Data_Ready.TryGetValue(serviceId, out var result) && result)
            {
                await CheckServiceDataReady(new List<int>() { serviceId });
            }
        }

        public bool GetSubserviceStatus(int Service_ID, int Subservice_ID) => Subservices_Data[Service_ID][Subservice_ID].Selected;

        public HashSet<int> TryGetProductForm(int? Service_ID, int? Package_ID, bool Is_Package)
        {
            if (Is_Package)
            {
                if (Packages_Rel_Cards.TryGetValue(Package_ID!.Value, out var result))
                {
                    return result;
                }
                return new HashSet<int>();
            }
            else
            {
                if (Services_Rel_Cards.TryGetValue(Service_ID!.Value, out var result))
                {
                    return result;
                }
                return new HashSet<int>();
            }
        }

        public T? GetElementValueAs<T>(int Card_ID, int Element_ID) => (T?)Shared_Element_Data_Storage[Card_ID][Element_ID];

        public void SetOptionsValidation(bool value, int Card_ID, int Element_ID)
        {
            if (Cards[Card_ID].Elements.Count == 1)
            {
                Cards[Card_ID].SetValidationState(value);
            }
            Cards[Card_ID].SetChipsValidation(Element_ID, value);
        }

        public async Task WriteSharedElementValue(object? value, int Card_ID, int Element_ID)
        {
            Shared_Element_Data_Storage[Card_ID][Element_ID] = value;
            await CheckServiceDataReady(Cards[Card_ID].From_Services);
        }

        public void TryBindSharedChipset(int Card_ID, int Element_ID)
        {
            if (Shared_Chip_Data_Storage.TryGetValue(Card_ID, out _))
            {
                if (!Shared_Chip_Data_Storage[Card_ID].TryGetValue(Element_ID, out _))
                {
                    Shared_Chip_Data_Storage[Card_ID][Element_ID] = null;
                }
            }
            else
            {
                Shared_Chip_Data_Storage[Card_ID] = new Dictionary<int, MudChip?>();
                Shared_Chip_Data_Storage[Card_ID][Element_ID] = null;
            }
        }

        private void AddPackageTariff(int packageID, PackageTariff tariff) => Packages_Tariffs[packageID] = tariff;

        private void AddServiceTariff(int serviceID, ServiceTariff tariff) => Services_Tariffs[serviceID] = tariff;

        public IDictionary<int, PackageTariff> GetPackagesTariffs() => Packages_Tariffs;

        public IDictionary<int, ServiceTariff> GetServicesTariffs() => Services_Tariffs;

        public IDictionary<int, IDictionary<int, StatefulSubservice>> GetSubservicesData() => Subservices_Data;

        public IDictionary<int, StatefulCard> GetCards() => Cards;

        public HashSet<int> GetServiceCards(int service_id) => Services_Rel_Cards[service_id] ?? new HashSet<int>();

        public HashSet<int> GetPackageCards(int package_id) => Packages_Rel_Cards[package_id] ?? new HashSet<int>();

        public IDictionary<int, bool> GetServiceDataReady() => Service_Data_Ready;

        public IDictionary<int, List<KeyValue>> GetServiceTariffFields() => Service_Tariff_Fields;

        public IDictionary<int, KeyValueInt> GetQuotationServices() => Quotation_Services;

        public IDictionary<int, IEnumerable<KeyValueInt>> GetQuotationPackageServices() => Quotation_Packages_Services;

        public IDictionary<int, int> GetPackagesReverseLookup() => Quotation_Packages_Services_Reverse;

        public bool GetQuotationInitialized() => Has_Been_Initialized;

        public void SetCardValidationState(bool value, int Card_ID)
        {
            if (Cards[Card_ID].GetChipsValidation().Any())
            {
                var validation = Cards[Card_ID].GetChipsValidation().Values.All(valid => valid);
                Cards[Card_ID].SetValidationState(value && validation);
            }
            else
            {
                Cards[Card_ID].SetValidationState(value);
            }
            NotifyStateChanged();
        }

        public bool GetCardValidationState(int Card_ID) => Cards[Card_ID].GetCardValidated();

        public bool AllReady() => Service_Data_Ready.Values.All(ready => ready);

        public bool ServiceComplete(int serviceID) => Service_Data_Ready.TryGetValue(serviceID, out var value) ? value : false;

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

        public MudChip? GetSharedChip(int Card_ID, int Element_ID) => Shared_Chip_Data_Storage[Card_ID][Element_ID];

        public void WriteSharedChip(MudChip? value, int Card_ID, int Element_ID) => Shared_Chip_Data_Storage[Card_ID][Element_ID] = value;

        public bool IsGloballyReadonly() => Global_Disable 
            ? true 
            : (!Has_Been_Initialized || Is_Calculating || Is_Saving || Block_Quotation);

        public bool GetQuotationFullyValid() => All_Valid_Tariffs;

        public bool GetQuotationPDFAvailable() => PDF_Available;

        /// <summary>
        /// Verifica que todos los items contengan servicio port capital
        /// </summary>
        /// <returns></returns>
        public bool AllItemsPortCapital()
        {
            bool services = true;
            bool packages = true;

            if (Quotation_Cart.GetServices() is not null && Quotation_Cart.GetServices().Any())
            {
                var uniqueTiposClientePC = Quotation_Cart.GetServices().Values.Select(service => service.TipoClientePC).Distinct().ToList();
                bool isValid = (uniqueTiposClientePC.Contains("A") && uniqueTiposClientePC.Count == 2) || uniqueTiposClientePC.Count == 1;

                services = Quotation_Cart.GetServices().Values.All(service => service.PortCapital);
                var subservices = Subservices_Data.SelectMany(subs => subs.Value.Values.Select(inner => new {
                    Service_ID = subs.Key,
                    inner.Subservice_ID,
                    inner.Selected
                }))
                .Where(subservice => subservice.Selected)
                .All(selected => Quotation_Cart.GetServices()[selected.Service_ID].Subservicios[selected.Subservice_ID].PortCapital);

                services = services && subservices /*&& isValid*/;
            }

            if (Quotation_Cart.GetPackages() is not null && Quotation_Cart.GetPackages().Any())
            {
                var uniqueTiposClientePC = Quotation_Cart.GetPackages().Values.Select(package => package.Paquete.TipoClientePC).Distinct().ToList();
                bool isValid = uniqueTiposClientePC.Contains("A") && uniqueTiposClientePC.Count == 2 || uniqueTiposClientePC.Count == 1;

                packages = Quotation_Cart.GetPackages().Values.All(package => package.Paquete.PortCapital);
                var package_subservices = Subservices_Data.SelectMany(subs => subs.Value.Values.Select(inner => new {
                    Service_ID = subs.Key,
                    inner.Subservice_ID,
                    inner.Selected
                }))
                .Where(subservice => subservice.Selected)
                .All(selected => Quotation_Cart.GetPackages()[Quotation_Packages_Services_Reverse[selected.Service_ID]].Servicios[selected.Service_ID].Subservicios[selected.Subservice_ID].PortCapital);

                packages = packages && package_subservices /*&& isValid*/;
            }

            return packages && services && IsValidTypeClientePC();
        }

        private bool IsValidTypeClientePC()
        {
            List<string> listTipoClientePC = ListTipoClientePC();
            return (listTipoClientePC.Contains("A") && listTipoClientePC.Count == 2) || listTipoClientePC.Count == 1;
        }

        private List<string> ListTipoClientePC()
        {
            List<string> TipoClientePC = new List<string>();
            if (Quotation_Cart.GetServices() is { } servicesDict && servicesDict.Any())
            {
                TipoClientePC.AddRange(servicesDict.Values.Select(service => service.TipoClientePC).Distinct().ToList());

                TipoClientePC.AddRange(Subservices_Data.SelectMany(subs => subs.Value.Values
                    .Where(inner => inner.Selected)
                    .Select(inner => Quotation_Cart.GetServices()[subs.Key].Subservicios[inner.Subservice_ID].TipoClientePC))
                    .Distinct()
                    .ToList());
            }

            if (Quotation_Cart.GetPackages() is { } packagesDict && packagesDict.Any())
            {
                TipoClientePC.AddRange(packagesDict.Values.Select(package => package.Paquete.TipoClientePC).Distinct().ToList());

                TipoClientePC.AddRange(Subservices_Data.SelectMany(subs => subs.Value.Values
                    .Where(inner => inner.Selected)
                    .Select(inner => Quotation_Cart.GetServices()[subs.Key].Subservicios[inner.Subservice_ID].TipoClientePC))
                    .Distinct()
                    .ToList());
            }

            return TipoClientePC.Distinct().ToList();
        }

        public bool IsExpress() => !ListTipoClientePC().Contains("N");

        public bool GetQuotationIsCalculating() => Is_Calculating;

        public async Task SaveQuotation(bool isDirectContract = false)
        {
            if ((!Is_Saving && All_Valid_Tariffs && !Block_Quotation && Has_Been_Initialized && !Global_Disable) || isDirectContract)
            {
                Is_Saving = true;
                NotifyStateChanged();
                var data = GeneratorsUtil.GenerateQuotationMiddleware(this);
                var result = await _dataAccessService.PostSaveQuotation(new SaveQuotationRequest()
                {
                    Data = data,
                    Booking_UUID = null
                });
                if (result is not null && result.Operation_Succeeded)
                {
                    Has_Saved = true;
                }// ELSE ERROR TODO
                Is_Saving = false;
                NotifyStateChanged();
            }
        }

        public async Task<bool> SaveFullQuotation(Guid? bookingUUID = null)
        {
            bool success = false;

            var data = GeneratorsUtil.GenerateQuotationMiddleware(this, true);
            var result = await _dataAccessService.PostSaveQuotation(new SaveQuotationRequest()
            {
                Data = data,
                Booking_UUID = bookingUUID
            });

            if (result is not null && result.Operation_Succeeded)
            {
                Invoice = result.Result;
                Has_Been_Initialized = false;
                Has_Saved = true;
                Global_Disable = true;
                PDF_Available = true;
                _shoppingCartHook.FullClear();
                success = true;
            } // ELSE ERROR TODO

            return success;
        }

        public async Task<bool> TrySaveFullQuotation(Guid? bookingUUID = null)
        {
            var success = false;
            if (!Global_Disable && All_Valid_Tariffs && !Block_Quotation && Has_Been_Initialized)
            {
                Is_Saving = true;
                NotifyStateChanged();

                success = await SaveFullQuotation(bookingUUID);

                Is_Saving = false;
                NotifyStateChanged();
            }
            return success;
        }

        public async Task<Stream?> GetQuotationPDF()
        {
            var stream = await _dataAccessService.PostDownloadPDF(UUID, LanguageUtil.getCurrentCulture(), Endpoint);
            return stream;
        }

        private bool ShouldTriggerPackage(int package_id)
            => Quotation_Packages_Services[package_id].All(service_kvp => Service_Data_Ready[service_kvp.Key]);

        public async Task CheckServiceDataReady(IEnumerable<int> serviceIDs)
        {
            bool anyChangesAggregate = false;

            foreach (var serviceID in serviceIDs)
            {
                bool result;
                result = Services_Rel_Cards.TryGetValue(serviceID, out var fields)
                    ? fields.All(card => Cards[card].GetCardValidated())
                    : false;

                Service_Data_Ready[serviceID] = result;

                var anyChanges = await TriggerCalculation(serviceID, result);
                anyChangesAggregate = anyChangesAggregate || anyChanges;
            }

            if (anyChangesAggregate)
            {
                await SaveQuotation();
            }

            NotifyStateChanged();
        }

        public async Task<bool> TriggerCalculation(int serviceID, bool data_ready)
        {
            try
            {
                if (!Is_Saving && !Block_Quotation && Has_Been_Initialized && !Global_Disable)
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
                        if (Quotation_Packages_Services_Reverse.TryGetValue(serviceID, out int package_id))
                        {
                            // Trigger package calculation
                            if (ShouldTriggerPackage(package_id))
                            {
                                await CalcularTarifasPaquete(package_id);
                                any_changes = true;
                                NotifyStateChanged();
                            }
                        }
                        // Single service
                        else
                        {
                            await CalcularTarifasServicio(serviceID);
                            any_changes = true;
                            NotifyStateChanged();
                        }
                    }
                    // Remover tarifa si los datos no están completos y si existe una tarifa
                    else
                    {
                        // Service comes from a package
                        if (Quotation_Packages_Services_Reverse.TryGetValue(serviceID, out int package_id))
                        {
                            Packages_Tariffs.Remove(package_id);
                        }
                        else if (Services_Tariffs.TryGetValue(serviceID, out _))
                        {
                            any_changes = true;
                            Services_Tariffs.Remove(serviceID);
                        }
                    }

                    if (Is_PortCapital && ISReadyQuotationPortCapital && AllItemsPortCapital())
                    {
                        await GetPortCapitalElements();
                        NotifyStateChanged();
                    }

                    Is_Calculating = false;

                    return any_changes;
                }

                return false;
            }
            catch (Exception ex)
            {
                Is_Calculating = false;
                return false;
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
            try
            {
                if (Service_Tariff_Fields.TryGetValue(serviceID, out List<KeyValue>? value))
                {
                    foreach (var card_fields_kvp in value)
                    {
                        fields.Add(card_fields_kvp.Value);
                        foreach (var item in card_fields_kvp.Value.Split(','))
                        {
                            switch (Cards[card_fields_kvp.Key].Elements[Convert.ToInt32(item)].Type_ID)
                            {
                                case EElementType.VALUES_LIST_UUID:
                                case EElementType.AUTOCOMPLE_LIST_UUID:
                                    {
                                        values.Add(GetElementValueAs<KeyValueUUID>(card_fields_kvp.Key, Convert.ToInt32(item))!.Key);
                                        break;
                                    }
                                case EElementType.OPTION_BOXES:
                                case EElementType.CHECK_BOX_UUID:
                                    {
                                        values.Add(GetElementValueAs<Guid>(card_fields_kvp.Key, Convert.ToInt32(item)));
                                        break;
                                    }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return new Tuple<IEnumerable<string>, IEnumerable<Guid>>(fields, values);
        }

        /// <summary>
        /// Obtiene los IDs de los subservicios de un servicio en específico y si estos afectan o no la tarifa
        /// </summary>
        /// <param name="serviceID">ID del servicio del cual se quieren obtener sus subservicios</param>
        /// <returns>Diccionario de IDs de subservicios y si estos afectan o no la tarifa</returns>
        public IEnumerable<StatefulSubservice> GetSubservices(int serviceID)
            => Subservices_Data.TryGetValue(serviceID, out IDictionary<int, StatefulSubservice>? subservices) && subservices is not null && subservices.Any()
                ? subservices.Values.Where(keyvalue => keyvalue.Selected)
                : Enumerable.Empty<StatefulSubservice>();

        private void ReCalculateAllValidTariffs()
        {
            var packages_conditions = Packages_Tariffs.Values.All(tariff => tariff.Valid_Tariff);
            var services_conditions = Services_Tariffs.Values.All(tariff => tariff.Valid_Tariff);
            All_Valid_Tariffs = packages_conditions && services_conditions;
        }

        /// <summary>
        /// Obtiene todos los valores numéricos asociados a un servicio o subservicio
        /// </summary>
        /// <param name="serviceID">ID del servicio o subservicio del cual se quieren obtener todos sus valores numéricos</param>
        /// <returns>Diccionario con el ID del campo y su valor correspondiente</returns>
        private IDictionary<int, object> GetAllValues(int serviceID)
        {
            if (Services_Rel_Cards.TryGetValue(serviceID, out var all_cards))
            {
                IDictionary<int, object> result = new Dictionary<int, object>();
                foreach (var card in all_cards)
                {
                    foreach (var element in Cards[card].Elements.Where(element => element.Value.Data_Type_ID == EDataType.INT || element.Value.Data_Type_ID == EDataType.DOUBLE || element.Value.Data_Type_ID == EDataType.DECIMAL).Select(element => element.Key))
                    {
                        result[element] = Shared_Element_Data_Storage[card][element]!;
                    }
                }
                return result;
            }
            else
            {
                return new Dictionary<int, object>();
            }
        }

        public async Task GetInvalidTariff(int serviceID) => await CalcularTarifasServicio(serviceID, false);

        private ClientQuotationData GetClientQuotationData(int? packageId = null) => new()
        {
            Client_ID = Client_ID,
            Location_ID = Location_ID,
            Money_Kind = Money_Kind,
            Service_Data = new Dictionary<int, ServiceQuotationData>(),
            Package_ID = packageId,
            NoCotizacion = IsServiceWithoutQuotation()
        };

        private ClientQuotationData GetServiceQuotationData(ClientQuotationData data, int elemetId, int? serviceVersion = null)
        {
            // Datos del servicio
            var fieldsValues = GetFieldsAndGuidValues(elemetId);
            var serviceValues = GetAllValues(elemetId);

            int elementVersion = serviceVersion ?? Quotation_Services[elemetId].Value;

            // Datos para subservicios
            var subservicesToQuote = GetSubservices(elemetId);

            ServiceQuotationData SetServiceQuotationData(Tuple<IEnumerable<string>, IEnumerable<Guid>> fieldsValues, IDictionary<int, object> serviceValues, int serviceVersion, IEnumerable<SubserviceQuotationData>? subserviceData = null) => new()
            {
                Fields_Values = new FieldsValuesWrapper(fieldsValues),
                Service_Values = serviceValues,
                Subservice_Data = subserviceData,
                Version = serviceVersion
            };

            if (subservicesToQuote.Any())
            {
                IList<SubserviceQuotationData> subservicesData = new List<SubserviceQuotationData>();
                foreach (var subserviceData in subservicesToQuote)
                {
                    var subserviceFieldValues = GetFieldsAndGuidValues(subserviceData.Subservice_ID);
                    var subserviceValues = GetAllValues(subserviceData.Subservice_ID);
                    subservicesData.Add(new()
                    {
                        Subservice_ID = subserviceData.Subservice_ID,
                        Effects_Tariff = subserviceData.Effects_Tariff,
                        Fields_Values = new FieldsValuesWrapper(subserviceFieldValues),
                        Service_Values = subserviceValues,
                        Version = subserviceData.Version
                    });
                }
                data.Service_Data[elemetId] = SetServiceQuotationData(fieldsValues, serviceValues, elementVersion, subservicesData);
            }
            else
            {
                data.Service_Data[elemetId] = SetServiceQuotationData(fieldsValues, serviceValues, elementVersion);
            }

            return data;
        }

        private async Task CalcularTarifasServicio(int serviceId, bool calculeted = true)
        {
            var data = GetClientQuotationData();
            GetServiceQuotationData(data, serviceId);

            // Post data to server and obtain a tariff calculation result
            var result = await _dataAccessService.GetCalculatedTariff(data);
            AddServiceTariff(serviceId, result.Tarifa_Servicio);

            SetMoneyKindFromServices(serviceId, result.Tarifa_Servicio);

            ReCalculateAllValidTariffs();

            UpdateTotals();
        }

        // <summary>
        // Sets the MoneyKind for a service based on the provided service data.
        // If there is more than one MoneyKind (MXN and USD), the service tariff will be invalidated.
        // If there is only one MoneyKind, it assigns the MoneyKind from the service data to the global Money_Kind.
        // </summary>
        // <param name="serviceId">The ID of the service to be updated.</param>
        // <param name="serviceData">The tariff data associated with the service.</param>
        private void SetMoneyKindFromServices(int serviceId, ServiceTariff serviceData)
        {
            // If the service tariff is not valid, exit the method.
            if (!serviceData.Valid_Tariff) return;

            Money_Kind = GetServicesTariffs().First(service => service.Value.Service_Tariff != null).Value.MoneyKind;
            foreach (var service in GetServicesTariffs().Values.Where(tariff => tariff.Valid_Tariff || tariff.Service_Tariff != null))
            {
                service.Valid_Tariff = service.MoneyKind == Money_Kind;
                AddServiceTariff(service.ID_Servicio, service);
            }
        }

        public async Task CalcularTarifasPaquete(int packageId)
        {
            var data = GetClientQuotationData(packageId);
            foreach (var packageServiceKvp in Quotation_Packages_Services[packageId])
            {
                GetServiceQuotationData(data, packageServiceKvp.Key, packageServiceKvp.Value);
            }

            // Post data to server and obtain a tariff calculation result
            TariffCalculationResult result = await _dataAccessService.GetCalculatedTariff(data);

            AddPackageTariff(packageId, result.Tarifa_Paquete);

            IsSingleMoneyKindInPackage(packageId);

            ReCalculateAllValidTariffs();

            UpdateTotals();
        }

        private void IsSingleMoneyKindInPackage(int packageId)
        {
            EMoney? firstValidMoneyKind = GetPackagesTariffs().First().Value.Tarifa_Base.Tarifas_Servicios.Values
                                        .Where(service => service.Valid_Tariff)
                                        .Select(service => service.MoneyKind)
                                        .FirstOrDefault();

            Money_Kind = firstValidMoneyKind == null || (int)firstValidMoneyKind! == 0 ? EMoney.MXN : firstValidMoneyKind.Value;

            foreach (var item in GetPackagesTariffs())
            {
                SetInvalidServiceTariffOnPackage(item.Key);
            }
        }

        // Obtener las tarifas del paquete y aplicar la consulta y luego invalidarlas
        private void SetInvalidServiceTariffOnPackage(int packageId)
        {
            void SetInvalidTariffOnElement(IEnumerable<ServiceTariff> element)
            {
                IEnumerable<ServiceTariff> InvalidTariffQuery(IEnumerable<ServiceTariff> element)
                    => element.Where(item => item.MoneyKind != Money_Kind && item.Valid_Tariff);

                foreach (var service in InvalidTariffQuery(element))
                {
                    service.Valid_Tariff = false;
                    service.Service_Tariff = null;
                    service.MoneyKind = Money_Kind;
                }
            }

            SetInvalidTariffOnElement(GetPackagesTariffs()[packageId].Tarifa_Base.Tarifas_Servicios.Values);
            SetInvalidTariffOnElement(GetPackagesTariffs()[packageId].Tarifa_Base.Tarifas_Servicios_Spread.Values);
        }

        private void UpdateTotals()
        {
            Totals.Subtotal = GetSubtotal();
            Totals.Packages_Discount = GetPackageDiscounts();
            Totals.Commercial_Discount = GetComDiscounts();
            Totals.Subtotal_W_Discount = GetDiscountedSubtotal();
            var should_refactor = false;
            if (Totals.Commercial_Discount.HasValue && Totals.Commercial_Discount < 0)
            {
                Totals.Commercial_Discount = null;
                should_refactor = true;
            }
            if (Totals.Packages_Discount.HasValue && Totals.Packages_Discount < 0)
            {
                Totals.Packages_Discount = null;
                should_refactor = true;
            }
            if (should_refactor && Totals.Subtotal_W_Discount.HasValue)
            {
                Totals.Subtotal = Totals.Subtotal_W_Discount.Value;
                Totals.Subtotal_W_Discount = null;
            }
            Totals.Total_Taxes = GetTaxTotals();
            Totals.Total = GetTotal(should_refactor);
            Totals.Total_To_Pay = Totals.Total;
            ISReadyQuotationPortCapital = true;
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

            if (GetServicesTariffs().Any())
            {
                foreach (var item in GetServicesTariffs().Where(tariff => tariff.Value.Valid_Tariff))
                {
                    services_subtotal += item.Value.Tax_Total ?? 0;
                    // Subservices_Tax_Totals IS NULL
                    services_subtotal += item.Value.Subservices_Tax_Totals?.Where(kvp => kvp.Value.HasValue).Sum(value => value.Value!.Value) ?? 0;
                }
            }

            return packages_subtotal + services_subtotal;
        }

        private decimal? GetDiscountedSubtotal()
        {
            decimal subtotalWithDiscount = 0;

            if (Totals.Commercial_Discount.HasValue)
            {
                subtotalWithDiscount += Totals.Commercial_Discount.Value;
            }

            if (Totals.Packages_Discount.HasValue)
            {
                subtotalWithDiscount += Totals.Packages_Discount.Value;
            }

            return subtotalWithDiscount == 0 ? null : Totals.Subtotal - subtotalWithDiscount;
        }

        private decimal GetSubtotal()
        {
            decimal packages_subtotal = 0;
            decimal services_subtotal = 0;

            if (GetPackagesTariffs().Any())
            {
                foreach (var item in GetPackagesTariffs().Values.Where(tariff => tariff.Valid_Tariff))
                {
                    packages_subtotal += item.Tarifa_Base.Tarifa_Subtotal;
                    foreach (var service_tariff in item.Tarifa_Base.Tarifas_Servicios.Values.Where(tariff => tariff.Valid_Tariff))
                    {
                        if (service_tariff.Service_Tariff.Tarifas_Subservicios is not null && service_tariff.Service_Tariff.Tarifas_Subservicios.Any())
                        {
                            packages_subtotal += service_tariff.Service_Tariff.Tarifas_Subservicios.Values.Sum(tariff => tariff.Tarifa_Subtotal);
                        }
                    }
                }
            }

            if (GetServicesTariffs().Any())
            {
                foreach (var item in GetServicesTariffs().Values.Where(tariff => tariff.Valid_Tariff))
                {
                    services_subtotal += item.Service_Tariff.Tarifa_Subtotal;
                    if (item.Service_Tariff.Tarifas_Subservicios is not null && item.Service_Tariff.Tarifas_Subservicios.Any())
                    {
                        services_subtotal += item.Service_Tariff.Tarifas_Subservicios.Values.Sum(tariff => tariff.Tarifa_Subtotal);
                    }
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

            if (GetServicesTariffs().Any())
            {
                foreach (var item in GetServicesTariffs().Where(tariff => tariff.Value.Valid_Tariff))
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
                return _packages_discounts != 0 ? _packages_discounts : null;
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

            if (GetServicesTariffs().Any())
            {
                foreach (var item in GetServicesTariffs().Values.Where(tariff => tariff.Valid_Tariff))
                {
                    if (item.Service_Tariff.Tarifa_Comercial is not null)
                    {
                        _com_discounts += item.Service_Tariff.Tarifa_Comercial.Descuento_Total_Calculado;
                    }

                    try
                    {
                        if (!item.Service_Tariff.Tarifas_Subservicios.Any()) continue;

                        foreach (var subservice in item.Service_Tariff.Tarifas_Subservicios.Values)
                        {
                            if(subservice.Tarifa_Comercial is not null)
                            {
                                _com_discounts += subservice.Tarifa_Comercial.Descuento_Total_Calculado;
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                    }
                    
                }
            }

            return _com_discounts != 0 ? _com_discounts : null;
        }

        private decimal GetTotal(bool should_refactor)
        {
            var _total = should_refactor ? Totals.Subtotal : GetAllTotals();
            if (Totals.Packages_Discount is not null && Totals.Packages_Discount != 0)
            {
                _total -= Totals.Packages_Discount.Value;
            }
            if (Totals.Commercial_Discount is not null && Totals.Commercial_Discount != 0)
            {
                _total -= Totals.Commercial_Discount.Value;
            }
            if (Totals.Total_Taxes is not null && Totals.Total_Taxes != 0)
            {
                _total += Totals.Total_Taxes.Value;
            }
            return _total;
        }

        #region PORT CAPITAL METHODS
        public async Task InitializedPortCapital()
        {
            Is_PortCapital = AllItemsPortCapital();

            if (Is_PortCapital)
            {
                await GetPortCapitalElements();
                ISReadyQuotationPortCapital = true;
            }
        }

        /// <summary>
        /// Recupera la cotizacion port capital
        /// </summary>
        /// <returns></returns>
        private async Task GetPortCapitalElements()
        {
            if (Totals.Total_To_Pay <= 0 || !Is_PortCapital)
            {
                ISReadyQuotationPortCapital = false;
                return;
            }

            RequestPortCapitalQuotationForm portCapitalRequestData = new RequestPortCapitalQuotationForm()
            {
                IDClient = Client_ID,
                Amount = Totals.Total_To_Pay,
                Currency_Key = Money_Kind.GetStringValue(),
                Is_Express = IsExpress(),
            };

            APIResult<PortCapitalResponse<PortCapitalQuotationResponse?>> result = await _dataAccessService.GetCalculatedTariffPortCapital(portCapitalRequestData);
            PortCapitalObj = result.Result;
            HasDataPortCapital = true;
        }

        public string GetStringMoneyKind() => Money_Kind.GetStringValue();

        public void SetIsPortCapital(bool IsPortCapital)
        {
            Is_PortCapital = IsPortCapital;
        }

        /// <summary>
        /// Verifica si laruta esta habilitado para port capital
        /// </summary>
        /// <returns></returns>
        public bool GetIsPortCapital()
        {
            return Is_PortCapital;
        }

        public PortCapitalResponse<PortCapitalQuotationResponse>? GetPortCapitalCollection()
        {
            return PortCapitalObj;
        }

        /// <summary>
        /// Indica que ya se ha obtenido la cotizacion de port capital
        /// </summary>
        /// <returns>Bool</returns>
        public bool GetHasDataPortCapital()
        {
            return HasDataPortCapital;
        }
        #endregion

        public bool IsServiceWithoutQuotation() =>  GetQuotationCart().IsNoShowQuotation();

        public bool IsServiceNoGenerateBookink() => GetQuotationCart().IsServiceNoGenerateBookink();

        public bool IsServiceRequireBillOfLanding() => GetQuotationCart().IsServiceRequireBillOfLanding();

        public QuotationServicesSettigs ServicesSettings() => new()
        {
            ServiceWithoutQuotation = IsServiceWithoutQuotation(),
            ServiceNoGenerateBookink = IsServiceNoGenerateBookink(),
            ServiceRequireBillOfLanding = IsServiceRequireBillOfLanding()
        };

    }
}
