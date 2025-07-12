using MudBlazor;
using PCG_FDF.Data.Entities;

namespace PCG_FDF.Data.Comparers
{
    public class ChipServiceComparer : IEqualityComparer<object>
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
                    return ((IService)((MudChip)a).Tag).GUID == ((IService)((MudChip)b).Tag).GUID;
                }
            }
        }
        public int GetHashCode(object x)
        {
            return (((IService)((MudChip)x).Tag).GUID.GetHashCode(), ((IService)((MudChip)x).Tag).FullName.GetHashCode(), ((IService)((MudChip)x).Tag).Id.GetHashCode()).GetHashCode();
        }
    }
}
