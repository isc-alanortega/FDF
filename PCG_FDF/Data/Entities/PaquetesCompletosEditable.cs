using PCG_ENTITIES.PCG;

namespace PCG_FDF.Data.Entities
{
    public class PaquetesCompletosEditable
    {
        public PaquetesEntidad Paquete { get; set; }

        public IDictionary<int, ServiciosPaqueteEditable> Servicios { get; set; }

        public PaquetesCompletosEditable()
        {
        }

        public PaquetesCompletosEditable(PaquetesEntidad paquete, IList<PaquetesServiciosCompletosEntidad> servicios)
        {
            Paquete = paquete;
            Servicios = new Dictionary<int, ServiciosPaqueteEditable>();
            if (servicios.Any())
            {
                Servicios = servicios.ToDictionary(service => service.Servicio.ID_Servicio, service => new ServiciosPaqueteEditable(service));
            }
        }

        public PaquetesCompletosEditable(PaquetesCompletosEntidad saved_package)
        {
            Paquete = saved_package.Paquete;
            Servicios = saved_package.Servicios.ToDictionary(service => service.Servicio.ID_Servicio, service => new ServiciosPaqueteEditable(service));
        }
    }
}
