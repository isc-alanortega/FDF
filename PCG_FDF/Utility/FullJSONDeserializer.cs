using Newtonsoft.Json.Linq;
using PCG_ENTITIES.Enums;
using PCG_ENTITIES.PCG_FDF.BookingEntities;
using PCG_ENTITIES.PCG_FDF.QuotationEntities;
using PCG_ENTITIES.PCG_FDF.UtilityEntities;


namespace PCG_FDF.Utility
{
    public static class FullJSONDeserializer
    {
        public static void RegenerateObjectData(BookingMiddleware source)
        {
            foreach (var element_data in source.Elements.Where(element_data => element_data.Value.Preloaded_Value is not null))
            {
                if (element_data.Value.Preloaded_Value is JObject data_source)
                {
                    source.Elements[element_data.Key].Preloaded_Value = GetDynamicObjectAsType(data_source, element_data.Value.Data_Type_ID);
                }
                else
                {
                    source.Elements[element_data.Key].Preloaded_Value = GetDynamicObjectAsType(element_data.Value.Preloaded_Value, element_data.Value.Data_Type_ID);
                }
            }

            foreach (var element_data in source.Elements.Where(element_data => element_data.Value.Complex_Data is not null && element_data.Value.Complex_Data.Any()))
            {
                foreach (var complex_data in element_data.Value.Complex_Data.Where(complex => complex.Value.Preloaded_Value is not null))
                {
                    if (element_data.Value.Complex_Data[complex_data.Key].Preloaded_Value is JObject data_source)
                    {
                        source.Elements[element_data.Key].Complex_Data[complex_data.Key].Preloaded_Value = GetDynamicObjectAsType(data_source, complex_data.Value.Data_Type_ID);
                    }
                    else
                    {
                        source.Elements[element_data.Key].Complex_Data[complex_data.Key].Preloaded_Value = GetDynamicObjectAsType(complex_data.Value.Preloaded_Value, complex_data.Value.Data_Type_ID);
                    }
                }

                foreach (var complex_data in element_data.Value.Complex_Data.Where(complex => complex.Value.Preloaded_List is not null))
                {
                    source.Elements[element_data.Key].Complex_Data[complex_data.Key].Preloaded_List = GetDynamicObjectAsType(complex_data.Value.Preloaded_List, complex_data.Value.Data_Type_ID);
                }
            }

            foreach (var element_data in source.Elements.Where(element_data => element_data.Value.Preloaded_List is not null))
            {
                source.Elements[element_data.Key].Preloaded_List = GetDynamicObjectAsType(source.Elements[element_data.Key].Preloaded_List, element_data.Value.Data_Type_ID);
            }

            foreach (var shared_data in source.Shared_Element_Data_Storage.Where(data => data.Value is not null))
            {
                if (shared_data.Value is JObject data_source)
                {
                    source.Shared_Element_Data_Storage[shared_data.Key] = GetDynamicObjectAsType(data_source, source.Elements[shared_data.Key].Data_Type_ID);
                }
                else
                {
                    source.Shared_Element_Data_Storage[shared_data.Key] = GetDynamicObjectAsType(shared_data.Value, source.Elements[shared_data.Key].Data_Type_ID);
                }
            }

            foreach (var unshared_data in source.Unshared_Element_Data_Storage)
            {
                foreach (var inner_data in unshared_data.Value)
                {
                    foreach (var final_data in inner_data.Value.Where(data => data.Value is not null))
                    {
                        if (final_data.Value is JObject data_source)
                        {
                            source.Unshared_Element_Data_Storage[unshared_data.Key][inner_data.Key][final_data.Key] = GetDynamicObjectAsType(data_source, source.Elements[final_data.Key].Data_Type_ID);
                        }
                        else
                        {
                            source.Unshared_Element_Data_Storage[unshared_data.Key][inner_data.Key][final_data.Key] = GetDynamicObjectAsType(final_data.Value, source.Elements[final_data.Key].Data_Type_ID);
                        }
                    }
                }
            }
        }

        public static void RegenerateObjectData(BookingInitializer source)
        {
            foreach (var element_data in source.Elements.Where(element_data => element_data.Value.Preloaded_Value is not null))
            {
                if (element_data.Value.Preloaded_Value is JObject data_source)
                {
                    source.Elements[element_data.Key].Preloaded_Value = GetDynamicObjectAsType(data_source, element_data.Value.Data_Type_ID);
                }
                else
                {
                    source.Elements[element_data.Key].Preloaded_Value = GetDynamicObjectAsType(element_data.Value.Preloaded_Value, element_data.Value.Data_Type_ID);
                }
            }

            foreach (var element_data in source.Elements.Where(element_data => element_data.Value.Complex_Data is not null && element_data.Value.Complex_Data.Any()))
            {
                foreach (var complex_data in element_data.Value.Complex_Data.Where(complex => complex.Value.Preloaded_Value is not null))
                {
                    if (element_data.Value.Complex_Data[complex_data.Key].Preloaded_Value is JObject data_source)
                    {
                        source.Elements[element_data.Key].Complex_Data[complex_data.Key].Preloaded_Value = GetDynamicObjectAsType(data_source, complex_data.Value.Data_Type_ID);
                    }
                    else
                    {
                        source.Elements[element_data.Key].Complex_Data[complex_data.Key].Preloaded_Value = GetDynamicObjectAsType(complex_data.Value.Preloaded_Value, complex_data.Value.Data_Type_ID);
                    }
                }

                foreach (var complex_data in element_data.Value.Complex_Data.Where(complex => complex.Value.Preloaded_List is not null))
                {
                    source.Elements[element_data.Key].Complex_Data[complex_data.Key].Preloaded_List = GetDynamicObjectAsType(complex_data.Value.Preloaded_List, complex_data.Value.Data_Type_ID);
                }
            }

            foreach (var element_data in source.Elements.Where(element_data => element_data.Value.Preloaded_List is not null))
            {
                source.Elements[element_data.Key].Preloaded_List = GetDynamicObjectAsType(source.Elements[element_data.Key].Preloaded_List, element_data.Value.Data_Type_ID);
            }
        }

        public static void RegenerateObjectData(QuotationInitializer source)
        {
            foreach (var card in source.Cards)
            {
                foreach (var element in card.Value.Elements)
                {
                    if (element.Value.Preloaded_List is not null)
                    {
                        source.Cards[card.Key].Elements[element.Key].Preloaded_List = GetDynamicObjectAsType(source.Cards[card.Key].Elements[element.Key].Preloaded_List, element.Value.Data_Type_ID);
                    }

                    if (element.Value.Preloaded_Value is not null)
                    {
                        if (element.Value.Preloaded_Value is JObject data_source)
                        {
                            source.Cards[card.Key].Elements[element.Key].Preloaded_Value = GetDynamicObjectAsType(data_source, element.Value.Data_Type_ID);
                        }
                        else
                        {
                            source.Cards[card.Key].Elements[element.Key].Preloaded_Value = GetDynamicObjectAsType(source.Cards[card.Key].Elements[element.Key].Preloaded_Value, element.Value.Data_Type_ID);
                        }
                    }
                }
            }
        }

        public static void RegenerateObjectData(QuotationMiddleware source)
        {
            foreach (var card in source.Cards)
            {
                foreach (var element in card.Value.Elements)
                {
                    if (element.Value.Preloaded_List is not null)
                    {
                        source.Cards[card.Key].Elements[element.Key].Preloaded_List = GetDynamicObjectAsType(source.Cards[card.Key].Elements[element.Key].Preloaded_List, element.Value.Data_Type_ID);
                    }

                    if (element.Value.Preloaded_Value is not null)
                    {
                        if (element.Value.Preloaded_Value is JObject data_source)
                        {
                            source.Cards[card.Key].Elements[element.Key].Preloaded_Value = GetDynamicObjectAsType(data_source, element.Value.Data_Type_ID);
                        }
                        else
                        {
                            source.Cards[card.Key].Elements[element.Key].Preloaded_Value = GetDynamicObjectAsType(source.Cards[card.Key].Elements[element.Key].Preloaded_Value, element.Value.Data_Type_ID);
                        }
                    }
                }
            }

            foreach (var saved_data in source.Shared_Element_Data_Storage)
            {
                foreach (var element_data in saved_data.Value)
                {
                    if (element_data.Value is not null)
                    {
                        if (element_data.Value is JObject data_source)
                        {
                            source.Shared_Element_Data_Storage[saved_data.Key][element_data.Key] = GetDynamicObjectAsType(data_source, source.Cards[saved_data.Key].Elements[element_data.Key].Data_Type_ID);
                        }
                        else
                        {
                            source.Shared_Element_Data_Storage[saved_data.Key][element_data.Key] = GetDynamicObjectAsType(source.Shared_Element_Data_Storage[saved_data.Key][element_data.Key], source.Cards[saved_data.Key].Elements[element_data.Key].Data_Type_ID);
                        }
                    }
                }
            }
        }

        private static object GetDynamicObjectAsType(object source, EDataType type)
        {
            switch (type)
            {
                case EDataType.STRING:
                    {
                        return (string)source;
                    }
                case EDataType.INT:
                    {
                        return TryParseIntValue(source);
                    }
                case EDataType.DOUBLE:
                    {
                        return (double)source;
                    }
                case EDataType.BOOL:
                    {
                        return (bool)source;
                    }
                case EDataType.UUID:
                case EDataType.OPTIONS_INITIALIZER:
                case EDataType.KEYVALUE_UUID:
                    {
                        return Guid.Parse((string)source);
                    }
                case EDataType.ONLYDATE:
                case EDataType.ONLYTIME:
                case EDataType.DATETIME:
                    {
                        return (DateTime)source;
                    }
                case EDataType.DECIMAL:
                    {
                        return (decimal)source;
                    }
                default:
                    {
                        throw new InvalidCastException();
                    }
            }
        }

        private static object GetDynamicObjectAsType(JObject source, EDataType type)
        {
            switch (type)
            {
                case EDataType.KEYVALUE:
                    {
                        return source.ToObject<KeyValue>()!;
                    }
                case EDataType.CONTAINER_CONTENTS:
                    {
                        return source.ToObject<ContainerContentsGroup>()!;
                    }
                case EDataType.ADDRESS_DATA:
                    {
                        return source.ToObject<AddressData>()!;
                    }
                case EDataType.KEYVALUE_UUID:
                    {
                        return source.ToObject<KeyValueUUID>()!;
                    }
                case EDataType.CONTAINER_TRIPS_ASSIGNMENT:
                    {
                        return source.ToObject<ContainerTripGroupFull>()!;
                    }
                case EDataType.CONTAINER_SIZES_TYPES:
                    {
                        return source.ToObject<ContainerSizesTypes>()!;
                    }
                case EDataType.CONTAINER_TRIPS_ASSIGNMENT_SHIPPING:
                    {
                        return source.ToObject<ContainerTripGroupShipping>()!;
                    }
                case EDataType.LOOSE_TRIPS_ASSIGNMENT_SHIPPING:
                    {
                        return source.ToObject<UnitTripGroupShipping>()!;
                    }
                case EDataType.IMO_SELECTION:
                    {
                        return source.ToObject<IMOData>()!;
                    }
                case EDataType.INSURANCE_SELECTION:
                    {
                        return source.ToObject<DocumentCheckData>()!;
                    }
                case EDataType.SAT_TABLE_TRIPLET:
                    {
                        return source.ToObject<SATTableTriplet>()!;
                    }
                case EDataType.ADDRESS_PRELOAD_DATA:
                    {
                        return source.ToObject<DireccionPrecargada>()!;
                    }
                case EDataType.SAT_MERCH_QUADRUPLET:
                    {
                        return source.ToObject<SATMerchQuadruplet>()!;
                    }
                case EDataType.CONTAINER_CONTENTS_PI:
                    {
                        return source.ToObject<ContainerContentsGroupPI>()!;
                    }
                case EDataType.CONTAINERS_ASSIGNMENT:
                    {
                        return source.ToObject<ContainerGroupLocal>()!;
                    }
                case EDataType.CONTAINERS_ASSIGNMENT_PI:
                    {
                        return source.ToObject<ContainerGroupLocalPI>()!;
                    }
                case EDataType.EMPTY_CONTAINERS_ASSIGNMENT:
                case EDataType.EMPTY_CONTAINERS_ASSIGNMENT_LOAD:
                    {
                        return source.ToObject<EmptyContainerGroup>()!;
                    }
                case EDataType.UNITS_ASSIGNMENT:
                    {
                        return source.ToObject<UnitTripGroup>()!;
                    }
                case EDataType.UNITS_ASSIGNMENT_PI:
                    {
                        return source.ToObject<UnitTripGroupPI>()!;
                    }
                case EDataType.LOOSE_UNITS_ASIGNMENT:
                    {
                        return source.ToObject<UnitTripGroup>()!;
                    }
                case EDataType.UNITS_ASSIGNMENT_VEHICLES:
                    {
                        return source.ToObject<UnitGroupVehicles>()!;
                    }
                case EDataType.TRAVELS_IN_SELECTION:
                    {
                        return source.ToObject<TravelsInData>()!;
                    }
                default:
                    {
                        throw new InvalidCastException();
                    }
            }
        }

        private static IEnumerable<object> GetDynamicObjectAsType(IEnumerable<object> source, EDataType type)
        {
            switch (type)
            {
                case EDataType.KEYVALUE:
                    {
                        return source.Select(jobj => ((JObject)jobj).ToObject<KeyValue>()!);
                    }
                case EDataType.OPTIONS_INITIALIZER:
                    {
                        return source.Select(jobj => ((JObject)jobj).ToObject<OptionBox>()!);
                    }
                case EDataType.KEYVALUE_UUID:
                    {
                        return source.Select(jobj => ((JObject)jobj).ToObject<KeyValueUUID>()!);
                    }
                case EDataType.SAT_TABLE_TRIPLET:
                    {
                        return source.Select(jobj => ((JObject)jobj).ToObject<SATTableTriplet>()!);
                    }
                case EDataType.ADDRESS_PRELOAD_DATA:
                    {
                        return source.Select(jobj => ((JObject)jobj).ToObject<DireccionPrecargada>()!);
                    }
                case EDataType.SAT_MERCH_QUADRUPLET:
                    {
                        return source.Select(jobj => ((JObject)jobj).ToObject<SATMerchQuadruplet>()!);
                    }
                default:
                    {
                        throw new InvalidCastException();
                    }
            }
        }

        private static int TryParseIntValue(object source)
        {
            if (source is int)
            {
                return (int)source;
            }
            if (source is long)
            {
                return (int)(long)source;
            }
            throw new InvalidOperationException();
        }
    }
}
