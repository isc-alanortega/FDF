using MudBlazor;
using PCG_ENTITIES.PCG;
using PCG_ENTITIES.PCG_FDF.UtilityEntities;
using System.Collections.Generic;

namespace PCG_FDF.Data.ComponentDI.Quotation
{
    public class ElementQuotationData : IDisposable
    {
        private int ELEMENT_ID;
        private string ELEMENT_NAME = "";
        private HashSet<int> from_services = new HashSet<int>();
        // Field ID, excluded GUIDs 
        private IDictionary<int, HashSet<Guid>> exclusions = new Dictionary<int, HashSet<Guid>>();
        private IDictionary<int, int?>? int_data = null;
        private IDictionary<int, double?>? double_data = null;
        private IDictionary<int, DateTime?>? dateTime_data = null;
        private IDictionary<int, string?>? string_data = null;
        private IDictionary<int, bool?>? bool_data = null;
        private IDictionary<int, Guid?>? guid_data = null;
        private IDictionary<int, MudChip?>? chip_data = null;
        // WE COULD ADJUST TO BE ABLE TO TAKE ANY KIND OF OBJECT SO THAT IT CAN BE USED IN ANY COMPONENT
        private IDictionary<int, LugaresEntidad>? complex_data = null;
        private HashSet<int> field_types = new HashSet<int>();
        private bool _all_answered = false;
        // Action that triggers a StateHasChanged event to rerender the component
        public event Action OnChange;
        private readonly QuotationDataCollection_LEGACY _parent_service;

        public ElementQuotationData(QuotationDataCollection_LEGACY parent)
        {
            _parent_service = parent;
        }

        public ElementQuotationData(MiddlewareElement saved_element, QuotationDataCollection_LEGACY parent)
        {
            ELEMENT_ID = saved_element.ELEMENT_ID;
            from_services = saved_element.from_services;
            _all_answered = saved_element._all_answered;
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
            _parent_service = parent;
        }

        public IDictionary<int, object> GetAllNumericData()
        {
            IDictionary<int, object> result = new Dictionary<int, object>();
            if (int_data is not null && int_data.Count != 0)
            {
                foreach (KeyValuePair<int, int?> item in int_data)
                {
                    result.Add(item.Key, item.Value!);
                }
            }
            if (double_data is not null && double_data.Count != 0)
            {
                foreach (KeyValuePair<int, double?> item in double_data)
                {
                    result.Add(item.Key, item.Value!);
                }
            }
            return result;
        }

        /// <summary>
        /// Dispara el evento de StateHasChanged y notifica al componente principal
        /// </summary>
        private void NotifyStateChanged() => OnChange?.Invoke();

        public void TryAddExclusion(int field_id, IEnumerable<Guid> options)
        {
            if (exclusions.TryGetValue(field_id, out var exclusion_values))
            {
                foreach (var exclusion in options)
                {
                    exclusion_values.Add(exclusion);
                }
            }
            else
            {
                exclusions[field_id] = options.ToHashSet();
            }
        }

        public IDictionary<int, HashSet<Guid>> GetElementExclusions()
        {
            return exclusions;
        }

        public HashSet<Guid> GetFieldExclusions(int field_id)
        {
            return exclusions.TryGetValue(field_id, out var field_exclusions) ? field_exclusions : new HashSet<Guid>();
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

        public void AddServiceReference(int serviceID)
        {
            from_services.Add(serviceID);
        }

        public void RemoveAssociation(int target)
        {
            from_services.Remove(target);
        }

        public HashSet<int> GetAssociations()
        {
            return from_services;
        }

        public void ElementHasGuid()
        {
            guid_data = new Dictionary<int, Guid?>();
        }

        public void ElementHasGuid(IEnumerable<int> fieldIDs)
        {
            if (guid_data is null)
            {
                guid_data = new Dictionary<int, Guid?>();
            }
            foreach (int ID in fieldIDs)
            {
                if (!guid_data.ContainsKey(ID))
                {
                    guid_data[ID] = null;
                }
            }
        }

        public void ElementHasChips(IEnumerable<int> fieldIDs)
        {
            if (chip_data is null)
            {
                chip_data = new Dictionary<int, MudChip?>();
            }
            foreach (int ID in fieldIDs)
            {
                if (!chip_data.ContainsKey(ID))
                {
                    chip_data[ID] = null;
                }
            }
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

        public void AddField(int fieldID, int type)
        {
            switch (type)
            {
                case 1:
                    if (int_data is null)
                    {
                        field_types.Add(type);
                        int_data = new Dictionary<int, int?>();
                    }
                    if (!int_data.ContainsKey(fieldID))
                    {
                        int_data[fieldID] = null;
                    }
                    break;
                case 2:
                    if (double_data is null)
                    {
                        field_types.Add(type);
                        double_data = new Dictionary<int, double?>();
                    }
                    if (!double_data.ContainsKey(fieldID))
                    {
                        double_data[fieldID] = null;
                    }
                    break;
                case 3:
                    if (dateTime_data is null)
                    {
                        field_types.Add(type);
                        dateTime_data = new Dictionary<int, DateTime?>();
                    }
                    if (!dateTime_data.ContainsKey(fieldID))
                    {
                        dateTime_data[fieldID] = null;
                    }
                    break;
                case 4:
                    if (string_data is null)
                    {
                        field_types.Add(type);
                        string_data = new Dictionary<int, string?>();
                    }
                    if (!string_data.ContainsKey(fieldID))
                    {
                        string_data[fieldID] = null;
                    }
                    break;
                case 5:
                    if (bool_data is null)
                    {
                        field_types.Add(type);
                        bool_data = new Dictionary<int, bool?>();
                    }
                    if (!bool_data.ContainsKey(fieldID))
                    {
                        bool_data[fieldID] = false;
                    }
                    break;
                case 6:
                    if (guid_data is null)
                    {
                        field_types.Add(type);
                        guid_data = new Dictionary<int, Guid?>();
                    }
                    if (!guid_data.ContainsKey(fieldID))
                    {
                        guid_data[fieldID] = null;
                    }
                    break;
            }
        }

        public HashSet<int> GetFieldTypes()
        {
            return field_types;
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

        public IDictionary<int, MudChip?>? GetChipFields()
        {
            return chip_data;
        }

        public async Task SetIntValue(int key, int? value)
        {
            if (!_parent_service.GetQuotationLocked())
            {
                int_data![key] = value;
                _all_answered = await CheckElementFields();
                await _parent_service.CheckServiceDataReady(from_services);
                NotifyStateChanged();
            }
        }

        public async Task SetDoubleValue(int key, double value)
        {
            if (!_parent_service.GetQuotationLocked())
            {
                double_data![key] = value;
                _all_answered = await CheckElementFields();
                await _parent_service.CheckServiceDataReady(from_services);
                NotifyStateChanged();
            }
        }

        public async Task SetDateTimeValue(int key, DateTime value)
        {
            if (!_parent_service.GetQuotationLocked())
            {
                dateTime_data![key] = value;
                _all_answered = await CheckElementFields();
                await _parent_service.CheckServiceDataReady(from_services);
                NotifyStateChanged();
            }
        }

        public async Task SetStringValue(int key, string? value)
        {
            if (!_parent_service.GetQuotationLocked())
            {
                string_data![key] = value;
                _all_answered = await CheckElementFields();
                await _parent_service.CheckServiceDataReady(from_services);
                NotifyStateChanged();
            }
        }

        public async Task SetBoolValue(int key, bool? value)
        {
            if (!_parent_service.GetQuotationLocked())
            {
                bool_data![key] = value;
                _all_answered = await CheckElementFields();
                await _parent_service.CheckServiceDataReady(from_services);
                NotifyStateChanged();
            }
        }

        public void InitGuidValue(int key, Guid? value)
        {
            if (!_parent_service.GetQuotationLocked())
            {
                guid_data![key] = value;
            }
        }

        public async Task SetGuidValue(int key, Guid? value)
        {
            if (!_parent_service.GetQuotationLocked())
            {
                guid_data![key] = value;
                _all_answered = await CheckElementFields();
                await _parent_service.CheckServiceDataReady(from_services);
                NotifyStateChanged();
            }
        }

        public async Task SetChipValue(int key, MudChip value)
        {
            if (!_parent_service.GetQuotationLocked())
            {
                chip_data![key] = value;
                _all_answered = await CheckElementFields();
                await _parent_service.CheckServiceDataReady(from_services);
                NotifyStateChanged();
            }
        }

        public async Task<bool> CheckElementFields()
        {
            bool complete = await Task.Run(() =>
            {
                bool _complete = true;
                // LINQ queries, if the given dict is not null (Element uses fields of that type)
                // And if there is null or unset data in one of those fields then _complete
                // Evaluates to false which means there is data missing
                if (int_data is not null && int_data.Values.Any(value => !value.HasValue))
                {
                    _complete = false;
                }
                if (double_data is not null && double_data.Values.Any(value => !value.HasValue))
                {
                    _complete = false;
                }
                if (dateTime_data is not null && dateTime_data.Values.Any(value => !value.HasValue))
                {
                    _complete = false;
                }
                if (string_data is not null && string_data.Values.Any(value => string.IsNullOrEmpty(value)))
                {
                    _complete = false;
                }
                if (bool_data is not null && bool_data.Values.Any(value => !value.HasValue))
                {
                    _complete = false;
                }
                if (guid_data is not null && guid_data.Values.Any(value => !value.HasValue))
                {
                    _complete = false;
                }
                if (chip_data is not null && chip_data.Values.Any(value => value is null))
                {
                    _complete = false;
                }
                return _complete;
            });
            return complete;
        }

        public bool AllFieldsAnswered()
        {
            return _all_answered;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
            from_services.Clear();
            field_types.Clear();
            int_data?.Clear();
            double_data?.Clear();
            dateTime_data?.Clear();
            string_data?.Clear();
            bool_data?.Clear();
            chip_data?.Clear();
            complex_data?.Clear();
            guid_data?.Clear();
        }
    }
}
