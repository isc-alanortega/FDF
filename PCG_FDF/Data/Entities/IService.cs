using PCG_ENTITIES.PCG;

namespace PCG_FDF.Data.Entities
{
    public interface IService
    {
        public Guid GUID { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Icon { get; set; }
        public string? helpTooltip { get; set; }
        public string ViewBox { get; set; }
        public IDictionary<int, SubserviciosEntidad> Subservicios { get; set; }
    }
}
