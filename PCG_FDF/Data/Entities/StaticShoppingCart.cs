using PCG_ENTITIES.PCG;
using PCG_ENTITIES.PCG_FDF.UtilityEntities;
using PCG_FDF.Data.ComponentDI;

namespace PCG_FDF.Data.Entities
{
    public class StaticShoppingCart
    {
        private IDictionary<int, ComplexService> Services;
        private IDictionary<int, PaquetesCompletosEditable> Packages;

        public StaticShoppingCart() { }

        public StaticShoppingCart(IDictionary<int, ComplexService> services, IDictionary<int, PaquetesCompletosEditable> packages)
        {
            Services = services;
            Packages = packages;
        }

        public StaticShoppingCart(ShoppingCartContainer currentShoppingCart)
        {
            // This looks super weird but we're basically making a deep copy of the DI's items such that we can later clear
            // The shopping cart DI object and not see those changes propagated here
            Services = currentShoppingCart.GetBillableList().Select(service => service with { }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            Packages = currentShoppingCart.GetPackages().Select(package => package with { }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public IEnumerable<int> GetAllPackageIDs() => Packages.Keys;

        public IEnumerable<KeyValueInt> GetServiceVersionPairs()
            => Services.Values.Select(service => new KeyValueInt()
            {
                Key = service.Id,
                Value = service.Version
            });

        public IDictionary<int, IEnumerable<KeyValueInt>> GetPackageServiceVersionPairs()
            => Packages.Values.ToDictionary
            (
                key => key.Paquete.ID,
                value => value.Servicios.Values.Where(service => service.Active).Select(service => new KeyValueInt() { Key = service.Id, Value = service.Version})
            );

        public IDictionary<int, IDictionary<int, StatefulSubservice>> GetSubservicesPairs()
        {
            IDictionary<int, IDictionary<int, StatefulSubservice>> result = new Dictionary<int, IDictionary<int, StatefulSubservice>>();

            //Dictionary<int, StatefulSubservice> ConvertSubservices(IDictionary<int, SubserviciosEntidad> subservices) => subservices.ToDictionary(
            //    sub => sub.Value.Id_Subservicio,
            //    sub => new StatefulSubservice
            //    {
            //        Subservice_ID = sub.Value.Id_Subservicio,
            //        Version = sub.Value.Version,
            //        Name = sub.Value.Nombre,
            //        Icon = sub.Value.Icon,
            //        ViewBox = sub.Value.Viewbox,
            //        Effects_Tariff = sub.Value.Afecta_Tarifa,
            //        Selected = false
            //    }
            //);

            if (Packages.Any())
            {
                foreach (var package in Packages)
                {
                    foreach (var service in package.Value.Servicios.Values)
                    {
                        if (service.Active && service.Subservicios.Any())
                        {
                            result[service.Id] = service.Subservicios.Values.ToDictionary(key => key.Id_Subservicio, value => new StatefulSubservice()
                            {
                                Subservice_ID = value.Id_Subservicio,
                                Version = value.Version,
                                Name = value.Nombre,
                                Icon = value.Icon,
                                ViewBox = value.Viewbox,
                                Effects_Tariff = value.Afecta_Tarifa,
                                Selected = false
                            });
                        }
                    }
                }
            }

            if (Services.Any())
            {
                foreach (var service in Services.Values)
                {
                    result[service.Id] = service.Subservicios.Values.ToDictionary(key => key.Id_Subservicio, value => new StatefulSubservice()
                    {
                        Subservice_ID = value.Id_Subservicio,
                        Version = value.Version,
                        Name = value.Nombre,
                        Icon = value.Icon,
                        ViewBox = value.Viewbox,
                        Effects_Tariff = value.Afecta_Tarifa,
                        Selected = false
                    });
                }
            }

            return result;
        }

        public IEnumerable<int> GetAllServiceIDs() => Services.Keys;

        public IDictionary<int, ICollection<int>> GetServicesSubservicesLookup()
            => Services.Values.ToDictionary(service => service.Id, subservice => subservice.Subservicios.Keys);

        public IDictionary<int, ICollection<int>> GetPackagesServicesSubservicesLookup()
            => Packages.Values.SelectMany(package => package.Servicios.Values).Where(service => service.Active).ToDictionary(service => service.Id, subservice => subservice.Subservicios.Keys);

        public IDictionary<int, HashSet<int>> GetPackagesServicesIDs()
            => Packages.Select(package => package.Value.Servicios).ToDictionary(services => services.First().Value.ID_Paquete, services => services.Keys.ToHashSet());

        public IDictionary<int, int> GetPackagesServicesReverseLookup()
            => Packages.Values.SelectMany(package => package.Servicios.Values).Where(service => service.Active).ToDictionary(service => service.Id, service => service.ID_Paquete);

        public StaticShoppingCart(MiddlewareShoppingCart savedData)
        {
            Services = savedData.Services is not null && savedData.Services.Any()
                ? savedData.Services.ToDictionary(serviceKvp => serviceKvp.Key, serviceKvp => new ComplexService(serviceKvp.Value))
                : new Dictionary<int, ComplexService>();
            
            Packages = savedData.Packages is not null && savedData.Packages.Any()
                ? savedData.Packages.ToDictionary(packageKvp => packageKvp.Key, packageKvp => new PaquetesCompletosEditable(packageKvp.Value))
                : new Dictionary<int, PaquetesCompletosEditable>();
        }

        public void ResetSelf()
        {
            Services.Clear();
            Packages.Clear();
        }

        public IDictionary<int, ComplexService> GetServices() => Services;

        public IDictionary<int, PaquetesCompletosEditable> GetPackages() => Packages;

        public void SetServices(IDictionary<int, ComplexService> services) => Services = services;

        public void SetPackages(IDictionary<int, PaquetesCompletosEditable> packages) => Packages = packages;

        public bool GetIsExpress()
            => GetServices().Values.Any(service => service.TipoClientePC == "E");


        public bool IsNoShowQuotation()
            => !Packages.Any() && GetServices().Values.Any() && GetServices().Values.All(service => service.NoMostrarCotizacion);

        public bool IsServiceNoGenerateBookink()
            => !Packages.Any() && GetServices().Values.Any() && GetServices().Values.All(child => child.NoDescargarBooking);

        public bool IsServiceRequireBillOfLanding()
            => !Packages.Any() && GetServices().Values.Any() && GetServices().Values.Count == 1 && GetServices().Values.All(child => child.RequireBillOfLanding);
    }
}
