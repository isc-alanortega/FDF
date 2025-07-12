using PCG_ENTITIES.PCG;
using PCG_ENTITIES.PCG_FDF.UtilityEntities;

namespace PCG_FDF.Data.ComponentDI.Quotation
{
    public class ReadOnlyElement : IDisposable
    {
        private int ELEMENT_ID;
        private string ELEMENT_NAME = "";
        private IDictionary<int, int?>? int_data = null;
        private IDictionary<int, double?>? double_data = null;
        private IDictionary<int, DateTime?>? dateTime_data = null;
        private IDictionary<int, string?>? string_data = null;
        private IDictionary<int, bool?>? bool_data = null;
        private IDictionary<int, Guid?>? guid_data = null;
        private IDictionary<int, HashSet<Guid>> exclusions;
        // WE COULD ADJUST TO BE ABLE TO TAKE ANY KIND OF OBJECT SO THAT IT CAN BE USED IN ANY COMPONENT
        private IDictionary<int, LugaresEntidad>? complex_data = null;
        private HashSet<int> field_types = new HashSet<int>();

        public ReadOnlyElement(MiddlewareElement saved_element, string language)
        {
            ELEMENT_ID = saved_element.ELEMENT_ID;
            ELEMENT_NAME = language == "ES" ? saved_element.element_name_es : saved_element.element_name_en;
            if (saved_element.int_data is not null && saved_element.int_data.Any())
            {
                int_data = saved_element.int_data;
            }
            if (saved_element.double_data is not null && saved_element.double_data.Any())
            {
                double_data = saved_element.double_data;
            }
            if (saved_element.dateTime_data is not null && saved_element.dateTime_data.Any())
            {
                dateTime_data = saved_element.dateTime_data;
            }
            if (saved_element.string_data is not null && saved_element.string_data.Any())
            {
                string_data = saved_element.string_data;
            }
            if (saved_element.bool_data is not null && saved_element.bool_data.Any())
            {
                bool_data = saved_element.bool_data;
            }
            if (saved_element.complex_data is not null && saved_element.complex_data.Any())
            {
                complex_data = saved_element.complex_data;
            }
            if (saved_element.guid_data is not null && saved_element.guid_data.Any())
            {
                guid_data = saved_element.guid_data;
            }
            field_types = saved_element.field_types;
            exclusions = saved_element.exclusions;
        }

        public void SetElementID(int elementId)
        {
            ELEMENT_ID = elementId;
        }

        public void SetElementName(string elementName)
        {
            ELEMENT_NAME = elementName;
        }

        public int GetElementID()
        {
            return ELEMENT_ID;
        }

        public string GetElementName()
        {
            return ELEMENT_NAME;
        }

        public IEnumerable<Guid> GetFieldExclusions(int field_id)
        {
            return exclusions.TryGetValue(field_id, out var field_exclusions) ? field_exclusions : Enumerable.Empty<Guid>();
        }

        public void ElementHasComplex(IEnumerable<int> fieldIDs)
        {
            if (complex_data is null)
            {
                complex_data = new Dictionary<int, LugaresEntidad>();
            }
            foreach (int id in fieldIDs)
            {
                if (!complex_data.ContainsKey(id))
                {
                    complex_data[id] = new LugaresEntidad();
                }
            }
        }

        public IDictionary<int, LugaresEntidad>? GetComplex()
        {
            return complex_data;
        }

        public IDictionary<int, int?>? GetIntFields()
        {
            return int_data;
        }

        public IDictionary<int, double?>? GetDoubleFields()
        {
            return double_data;
        }

        public IDictionary<int, DateTime?>? GetDateTimeFields()
        {
            return dateTime_data;
        }

        public IDictionary<int, string?>? GetStringFields()
        {
            return string_data;
        }

        public IDictionary<int, bool?>? GetBoolFields()
        {
            return bool_data;
        }
        public IDictionary<int, Guid?>? GetGuidFields()
        {
            return guid_data;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
            field_types.Clear();
            int_data?.Clear();
            double_data?.Clear();
            dateTime_data?.Clear();
            string_data?.Clear();
            bool_data?.Clear();
            complex_data?.Clear();
            guid_data?.Clear();
        }
    }
}
