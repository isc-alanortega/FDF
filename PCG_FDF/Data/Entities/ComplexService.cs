using PCG_ENTITIES.PCG;
using System.Collections;

namespace PCG_FDF.Data.Entities
{
    public class ComplexService : IService, IEnumerable<ComplexService>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Icon { get; set; }
        public string? helpTooltip { get; set; }
        public string? Alias { get; set; }
        public string? Calculo_Cantidad { get; set; }
        public string ViewBox { get; set; }
        public string Code { get; set; }
        public bool PortCapital { get; set; }
        public int Version { get; set; }
        public IDictionary<int, SubserviciosEntidad> Subservicios { get; set; }
        public Guid GUID { get; set; }
        public int level { get; set; }
        public bool isActionable { get; set; }
        public ComplexService? Parent { get; set; }
        public string TipoClientePC { get; set; }
        public bool NoDescargarBooking { get; set; }
        public bool NoMostrarCotizacion { get; set; }
        public bool RequireBillOfLanding { get; set; }
        public IDictionary<Guid, ComplexService> Children { get; set; }

        private int amount = 0;

        public ComplexService(ServiciosCompletosEntidad saved_service)
        {
            GUID = Guid.NewGuid();
            Id = saved_service.Servicio.ID;
            Name = saved_service.Servicio.Nombre_corto;
            FullName = saved_service.Servicio.Nombre;
            Code = saved_service.Servicio.Codigo;
            Version = saved_service.Servicio.Version;
            Alias = saved_service.Servicio.Alias;
            Calculo_Cantidad = saved_service.Servicio.CalculoCantidad;
            PortCapital = saved_service.Servicio.PortCapital;
            Icon = saved_service.Servicio.Icon;
            level = 0;
            isActionable = true;
            helpTooltip = saved_service.Servicio.Descripcion;
            ViewBox = saved_service.Servicio.Viewbox;
            Parent = null;
            Children = new Dictionary<Guid, ComplexService>();
            TipoClientePC = saved_service.Servicio.TipoClientePC;
            NoDescargarBooking = saved_service.Servicio.NoDescargarBooking;
            NoMostrarCotizacion = saved_service.Servicio.NoMostrarCotizacion;
            RequireBillOfLanding = saved_service.Servicio.RequireBillOfLanding;
            Subservicios = saved_service.Subservicios.ToDictionary(subservice => subservice.Id_Subservicio, subservice => subservice);
        }

        public ComplexService(int id, string code, int version, string name, string fullName, string? alias, string? calculo_cantidad, string icon, string? viewbox, int level, bool isActionable, string? helpTooltip, bool _PortCapital, ComplexService? parent, IDictionary<Guid, ComplexService> children, IList<SubserviciosEntidad> subservicios, string tipoClientePC = "N", bool noDescargarBooking = false, bool noShowQuotation = false, bool requireBillOfLading = false)
        {
            Id = id;
            Name = name;
            Alias = alias;
            Calculo_Cantidad = calculo_cantidad;
            FullName = fullName;
            Code = code;
            Version = version;
            Icon = icon;
            if (string.IsNullOrEmpty(viewbox))
            {
                ViewBox = "0 0 24 24";
            }
            else
            {
                ViewBox = viewbox;
            }
            this.level = level;
            this.isActionable = isActionable;
            this.helpTooltip = helpTooltip;
            Parent = parent;
            Children = children;
            PortCapital = _PortCapital;
            tipoClientePC = tipoClientePC;
            Subservicios = subservicios.ToDictionary(subservice => subservice.Id_Subservicio, subservice => subservice);
            GUID = Guid.NewGuid();
            NoDescargarBooking = noDescargarBooking;
            NoMostrarCotizacion = noShowQuotation;
            RequireBillOfLanding = requireBillOfLading;
        }

        public void addChild(ComplexService child)
        {
            child.Parent = this;
            Children.Add(child.GUID, child);
        }

        public IEnumerator<ComplexService> GetEnumerator()
        {
            return Children.Values.GetEnumerator();
        }

        public IDictionary<Guid, ComplexService> getChildren(ComplexService node)
        {
            return node.Children;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int getAmount()
        {
            return amount;
        }

        public void setAmount(int n)
        {
            amount = n;
        }
    }
}
