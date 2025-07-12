using MudBlazor;
using PCG_ENTITIES.PCG_FDF.TrackingEntities;

namespace PCG_FDF.Data.Comparers
{
    public class ChipContractInfoComparer : IEqualityComparer<object>
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
                    return ((ContractInfo)((MudChip)a).Tag).Contract_UUID == ((ContractInfo)((MudChip)b).Tag).Contract_UUID;
                }
            }
        }

        public int GetHashCode(object x)
        {
            return ((ContractInfo)((MudChip)x).Tag).Contract_UUID.GetHashCode();
        }
    }
}
