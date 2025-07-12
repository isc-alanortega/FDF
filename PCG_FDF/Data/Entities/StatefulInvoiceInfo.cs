using MudBlazor;
using PCG_ENTITIES.Enums;
using PCG_ENTITIES.PCG_FDF.UtilityEntities;

namespace PCG_FDF.Data.Entities
{
    public class StatefulInvoiceInfo
    {
        public InvoiceInfo Invoice { get; set; }
        public bool Selected { get; set; }
        public EInvoicePaymentType? Payment_Type { get; set; }
        public MudChip? Selected_Chip { get; set; }
        public decimal? Payment { get; set; }
    }
}
