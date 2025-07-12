using MudBlazor;
using PCG_ENTITIES.Enums;

namespace PCG_FDF.Data.Comparers
{
    public class ChipEInvoiceComparer : IEqualityComparer<object>
    {
        public new bool Equals(object? a, object? b)
        {
            if (a == null && b == null)
            {
                return true;
            }
            else
            {
                if (a == null || b == null)
                {
                    return false;
                }
                else
                {
                    return ((EInvoicePaymentType)((MudChip)a).Tag!) == ((EInvoicePaymentType)((MudChip)b).Tag!);
                }
            }
        }

        public int GetHashCode(object x)
        {
            return ((EInvoicePaymentType)((MudChip)x).Tag!).GetHashCode();
        }
    }
}
