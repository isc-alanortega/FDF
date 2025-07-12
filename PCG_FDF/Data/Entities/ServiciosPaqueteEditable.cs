using PCG_ENTITIES.PCG;

namespace PCG_FDF.Data.Entities
{
    public class ServiciosPaqueteEditable : IService
    {
        public Guid GUID { get; set; }
        public int ID_Paquete { get; set; }
        public int Secuencia { get; set; }
        public int Id { get; set; }
        public int Secuencia_universal { get; set; }
        public string FullName { get; set; }
        public string Name { get; set; }
        public string? helpTooltip { get; set; }
        public string Icon { get; set; }
        public string ViewBox { get; set; }
        public bool EsCore { get; set; }
        public string Codigo { get; set; }
        public int Version { get; set; }
        public bool Habilitado { get; set; }
        public bool Active { get; set; }
        public IDictionary<int, SubserviciosEntidad> Subservicios { get; set; }

        public ServiciosPaqueteEditable(PaquetesServiciosCompletosEntidad servicio)
        {
            GUID = Guid.NewGuid();
            ID_Paquete = servicio.Servicio.ID_Paquete;
            Secuencia = servicio.Servicio.Secuencia;
            Id = servicio.Servicio.ID_Servicio;
            Secuencia_universal = servicio.Servicio.Secuencia_universal;
            FullName = servicio.Servicio.Nombre_Servicio;
            Name = servicio.Servicio.Nombre_Corto_Servicio;
            helpTooltip = servicio.Servicio.Descripcion_Servicio;
            Icon = servicio.Servicio.Icon;
            ViewBox = servicio.Servicio.Viewbox;
            EsCore = servicio.Servicio.EsCore;
            Codigo = servicio.Servicio.Codigo;
            Version = servicio.Servicio.Version;
            Habilitado = servicio.Servicio.Habilitado;
            Subservicios = servicio.Subservicios.ToDictionary(subservice => subservice.Id_Subservicio, subservice => subservice);
            Active = true;
        }
    }
}
