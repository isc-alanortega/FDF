using PCG_ENTITIES.PCG_FDF.QuotationEntities;

namespace PCG_FDF.Data.Entities
{
    public class StatefulSubservice
    {
        /// <summary>
        /// ID del subservicio
        /// </summary>
        public int Subservice_ID { get; set; }
        /// <summary>
        /// Version del subservicio
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// Nombre del subservicio
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Ícono del subservicio
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// Viewbox del ícono
        /// </summary>
        public string ViewBox { get; set; }
        /// <summary>
        /// Indica si está seleccionado para cotizar
        /// </summary>
        public bool Selected { get; set; }
        /// <summary>
        /// Indica si afecta la tarifa
        /// </summary>
        public bool Effects_Tariff { get; set; }

        public StatefulSubservice() { }

        public StatefulSubservice(StatefulSubserviceMiddleware source)
        {
            Subservice_ID = source.Subservice_ID;
            Version = source.Version;
            Name = source.Name;
            Icon = source.Icon;
            ViewBox = source.ViewBox;
            Selected = source.Selected;
            Effects_Tariff = source.Effects_Tariff;
        }
    }
}
