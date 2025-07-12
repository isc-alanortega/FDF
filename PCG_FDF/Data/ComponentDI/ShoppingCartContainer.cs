using MudBlazor;
using PCG_ENTITIES.PCG;
using PCG_FDF.Data.DataAccess;
using PCG_FDF.Data.Entities;

namespace PCG_FDF.Data.ComponentDI
{
    public class ShoppingCartContainer
    {
        private int _totalItems = 0;
        private IDictionary<int, IList<ComplexService>> _serviceTraces = new Dictionary<int, IList<ComplexService>>();
        private IDictionary<int, PaquetesCompletosEditable> _packages = new Dictionary<int, PaquetesCompletosEditable>();
        private IDictionary<int, ComplexService> _servicesToBill = new Dictionary<int, ComplexService>();
        private PaquetesCompletosEntidad? _suggestion = null;

        // Action that triggers a StateHasChanged event to rerender the component
        public event Action _onChange;

        // injections
        private readonly PCG_FDF_DB _dataAccessService;
        private readonly WhiteLabelManager _whiteLabel;

        public ShoppingCartContainer(PCG_FDF_DB dataAccessService, WhiteLabelManager whiteLabel)
        {
            _dataAccessService = dataAccessService;
            _whiteLabel = whiteLabel;
        }

        /// <summary>
        /// Dispara el evento de StateHasChanged y notifica al componente principal
        /// </summary>
        private void NotifyStateChanged() => _onChange?.Invoke();

        public void AddServiceTrace(IList<ComplexService> trace)
        {
            trace.RemoveAt(0);
            _serviceTraces[trace[trace.Count - 1].Id] = trace;
            IncreaseTotalItems();
            NotifyStateChanged();
        }

        public void UnsetSuggestion()
        {
            _suggestion = null;
            NotifyStateChanged();
        }

        public PaquetesCompletosEntidad? GetSuggestion() => _suggestion;

        public void AddCleanServiceTrace(IList<ComplexService> trace)
        {
            _serviceTraces[trace[trace.Count - 1].Id] = trace;
            IncreaseTotalItems();
            NotifyStateChanged();
        }

        public async Task ServiceCheck(string language)
        {
            if (_serviceTraces.Keys.Count > 1)
            {
                var services = string.Join(',', _serviceTraces.Keys);
                _suggestion = await _dataAccessService.GetPaqueteMatch(language, services, _whiteLabel.GetPageID());
            }
            else
            {
                _suggestion = null;
            }
            NotifyStateChanged();
        }

        public void AddSuggestion()
        {
            foreach (var service in _suggestion!.Servicios.Where(service => _serviceTraces.TryGetValue(service.Servicio.ID_Servicio, out _)))
            {
                RemoveAt(service.Servicio.ID_Servicio);
                DecreaseTotalItems();
            }
            var temp = _suggestion;
            _suggestion = null;
            AddPackage(temp);
        }

        public void AddPackage(PaquetesCompletosEntidad package)
        {
            _packages[package.Paquete.ID] = new PaquetesCompletosEditable(package.Paquete, package.Servicios);
            IncreaseTotalItems();
            NotifyStateChanged();
        }

        public void FullClear()
        {
            _packages.Clear();
            _serviceTraces.Clear();
            _totalItems = 0;
            _suggestion = null;
            _servicesToBill.Clear();
        }

        public void RemoveAt(int key)
        {
            _serviceTraces.Remove(key);
            NotifyStateChanged();
        }

        public void RemovePackageAt(int key)
        {
            _packages.Remove(key);
            NotifyStateChanged();
        }

        public string? CheckPackageServices(int serviceId)  
            => _packages.Values.Select(package => package).FirstOrDefault(pack => pack.Servicios.TryGetValue(serviceId, out _))?.Paquete?.Nombre_Paquete;

        public string? CheckPackageServicesAdd(PaquetesCompletosEntidad paquete)
        {
            foreach (var servicio in paquete.Servicios)
            {
                var service = _serviceTraces.Values.Select(services => services[services.Count - 1]).FirstOrDefault(service => service.Id == servicio.Servicio.ID_Servicio);
                if (service is not null)
                {
                    return service.Name;
                }
            }
            return null;
        }

        public string? CheckPackageAdd(PaquetesCompletosEntidad paquete)
        {
            foreach (PaquetesServiciosCompletosEntidad servicio in paquete.Servicios)
            {
                var return_package = _packages.Values.Select(package => package).FirstOrDefault(pack => pack.Servicios.TryGetValue(servicio.Servicio.ID_Servicio, out _));
                if (return_package is not null)
                {
                    return return_package.Paquete.Nombre_Corto_Paquete;
                }
            }
            return null;
        }

        public IDictionary<int, IList<ComplexService>> GetServiceTraces() => _serviceTraces;

        public IDictionary<int, ComplexService> GetBillableList() => _servicesToBill;

        public void AddServiceToBill(ComplexService service) => _servicesToBill.Add(service.Id, service);

        public int GetTotalItems() => _totalItems;
        
        public void SetTotalItems(int n) => _totalItems = n;

        public void IncreaseTotalItems() => _totalItems++;

        public void DecreaseTotalItems() => _totalItems--;
        
        public void GenerateBillingList()
        {
            // Ensures this is kept updated in every operation
            _servicesToBill.Clear();
            foreach (var trace in _serviceTraces.Select(trace => trace.Value))
            {
                _servicesToBill[trace[trace.Count - 1].Id] = trace[trace.Count - 1];
            }
        }

        public IDictionary<int, PaquetesCompletosEditable> GetPackages() => _packages;

        public IDictionary<int, List<BreadcrumbItem>> GetFullTrace()
        {
            IDictionary<int, List<BreadcrumbItem>> _dict = new Dictionary<int, List<BreadcrumbItem>>();
            foreach (KeyValuePair<int, IList<ComplexService>> trace in _serviceTraces)
            {
                var _items = new List<BreadcrumbItem>();
                foreach (ComplexService service in trace.Value)
                {
                    _items.Add(new BreadcrumbItem(service.Name, null, true));
                }
                _dict[trace.Key] = _items;
            }
            return _dict;
        }

        public bool IsNoShowQuotation() => !_packages.Any() && _servicesToBill.Values.All(child => child.NoMostrarCotizacion);
    }
}
