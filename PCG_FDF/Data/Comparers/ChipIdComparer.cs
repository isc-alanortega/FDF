using MudBlazor;

namespace PCG_FDF.Data.Comparers
{
    public class ChipIdComparer : IEqualityComparer<object>
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
                    return ((int)((MudChip)a).Tag) == ((int)((MudChip)b).Tag);
                }
            }
        }
        public int GetHashCode(object x)
        {
            return ((int)((MudChip)x).Tag).GetHashCode();
        }
    }
}
