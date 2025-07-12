using MudBlazor;

namespace PCG_FDF.Data.Comparers
{
    public class ChipUUIDComparer : IEqualityComparer<object>
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
                    return ((Guid)((MudChip)a).Tag) == ((Guid)((MudChip)b).Tag);
                }
            }
        }

        public int GetHashCode(object x)
        {
            return ((Guid)((MudChip)x).Tag).GetHashCode();
        }
    }
}
