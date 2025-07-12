using MudBlazor;

namespace PCG_FDF.Utility
{
    public class CustomUUIDToBoolConverter : BoolConverter<Guid>
    {
        public CustomUUIDToBoolConverter(Guid UUID_True, Guid UUID_False)
        {
            TrueUUID = UUID_True;
            FalseUUID = UUID_False;
            SetFunc = OnSet;
            GetFunc = OnGet;
        }

        private Guid TrueUUID { get; set; }
        private Guid FalseUUID { get; set; }

        private Guid OnGet(bool? value) => value == null ? FalseUUID : (value!.Value ? TrueUUID : FalseUUID);

        private bool? OnSet(Guid arg)
        {
            try
            {
                if (arg == TrueUUID)
                {
                    return true;
                }
                if (arg == FalseUUID)
                {
                    return false;
                }
                return false;
            }
            catch (FormatException e)
            {
                UpdateSetError("Conversion error: " + e.Message);
                return false;
            }
        }
    }
}
