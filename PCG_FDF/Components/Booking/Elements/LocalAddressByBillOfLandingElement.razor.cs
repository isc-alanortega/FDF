//using Microsoft.AspNetCore.Components;
//using PCG_ENTITIES.PCG_FDF.UtilityEntities;
//using PCG_FDF.Data.DataAccess;
//using System.Text.RegularExpressions;

//namespace PCG_FDF.Components.Booking.Elements
//{
//    public partial class LocalAddressByBillOfLandingElement
//    {
//        #region Injects
//        [Inject] PCG_FDF_DB DataAccess { get; set; }
//        #endregion

//        private bool CanGetList { get; set; } = false;
//        private Func<DireccionPrecargada, string> AddressConverter = add => add?.Address_Name;
//        private DireccionPrecargada? SelectedAddress { get; set; } = null;

//        private int? AddressType { get; set; } = null;


//        #region BLAZOR
//        protected async override Task OnInitializedAsync()
//        {
//            if (ElementData.Validations.TryGetValue("ADDRESSTYPE", out var type))
//            {
//                AddressType = Convert.ToInt32(type.Validation_Value);
//            }

//            if (IsCollection)
//            {
//                if (BookingData.GetUnsharedStorage()[SectionData.Key][SectionData.Value.Keys.First()][ElementData.Element_ID] is null)
//                {
//                    await BookingData.WriteUnsharedElementValue(new AddressData(), SectionData.Key, SectionData.Value.Keys.First(), ElementData.Element_ID);
//                }
//                await SetComplexElementValid(ValidateElement());
//            }
//            else
//            {
//                if (BookingData.GetSharedStorage()[ElementData.Element_ID] is null)
//                {
//                    await BookingData.WriteSharedElementValue(new AddressData(), ElementData.Element_ID);
//                }
//                await SetComplexElementValid(ValidateElement());
//            }
//            await base.OnInitializedAsync();
//        }
//        #endregion

//        #region VALIDATE
//        private bool ValidateElement(AddressData? source = null)
//        {
//            if (!ElementData.Required)
//            {
//                return true;
//            }

//            var current_source = source ?? GetSource();

//            if (current_source is null)
//            {
//                return false;
//            }

//            if (string.IsNullOrEmpty(current_source.Post_Code) || string.IsNullOrWhiteSpace(current_source.Post_Code))
//            {
//                return false;
//            }

//            if (string.IsNullOrEmpty(current_source.Address) || string.IsNullOrWhiteSpace(current_source.Address))
//            {
//                return false;
//            }

//            if (current_source.Neighborhood is null)
//            {
//                return false;
//            }

//            if (current_source.City is null)
//            {
//                return false;
//            }

//            if (current_source.State is null)
//            {
//                return false;
//            }

//            if (current_source.Country is null)
//            {
//                return false;
//            }

//            return true;
//        }

//        public static bool IsValidRfc(string rfc)
//        {
//            if (string.IsNullOrWhiteSpace(rfc)) return false;

//            var regex = new Regex(@"^([A-ZÑ&]{3,4})(\d{2})(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])[A-Z\d]{2}[A\d0-9]$", RegexOptions.IgnoreCase);
//            return regex.IsMatch(rfc);
//        }
//        #endregion

//        private async Task TrySelectPreloadedAddress(DireccionPrecargada? value)
//        {
//            SelectedAddress = value;
//            if (SelectedAddress is null)
//            {
//                return;
//            }

//            var queryRequest = new DynamicQueryRequestBooking()
//            {
//                Booking_UUID = BookingData.UUID,
//                Client_ID = BookingData.Client_ID,
//                User_ID = BookingData.User_ID,
//                Query_ID = ElementData.Complex_Data["NEIGHBORHOOD"].Query_ID,
//                Language = BookingData.GetBookingLanguage(),
//                Splashed_Parameters = new Dictionary<string, object?>()
//                {
//                    {"PostCode", SelectedAddress.Zip_Code }
//                },
//                Query = null
//            };

//            var neighborhoodResult = await DataAccess.PostLeerQuery(queryRequest);

//            queryRequest.Query_ID = ElementData.Complex_Data["CITY"].Query_ID;
//            var cityResult = await DataAccess.PostLeerQuery(queryRequest);

//            queryRequest.Query_ID = ElementData.Complex_Data["STATE"].Query_ID;
//            var stateResult = await DataAccess.PostLeerQuery(queryRequest);

//            queryRequest.Query_ID = ElementData.Complex_Data["COUNTRY"].Query_ID;
//            var countryResult = await DataAccess.PostLeerQuery(queryRequest);

//            if (
//                !(neighborhoodResult!.Operation_Succeeded
//                && cityResult!.Operation_Succeeded
//                && stateResult!.Operation_Succeeded
//                && countryResult!.Operation_Succeeded
//                && neighborhoodResult.Result.Any()
//                && cityResult.Result.Any()
//                && stateResult.Result.Any()
//                && countryResult.Result.Any())
//            )
//            {
//                return;
//            }

//            ElementData.Complex_Data["NEIGHBORHOOD"].Preloaded_List = neighborhoodResult.Result;
//            ElementData.Complex_Data["CITY"].Preloaded_List = cityResult.Result;
//            ElementData.Complex_Data["STATE"].Preloaded_List = stateResult.Result;
//            ElementData.Complex_Data["COUNTRY"].Preloaded_List = countryResult.Result;

//            //await TryModifyField("NEIGHBORHOOD", neighborhood_result.Result.FirstOrDefault(neighborhood => neighborhood.Key == SelectedAddress.Neighborhood_ID));
//            //await TryModifyField("CITY", city_result.Result.First());
//            //await TryModifyField("STATE", state_result.Result.First());
//            //await TryModifyField("COUNTRY", country_result.Result.First());
//            //await TryModifyField("POSTCODE", SelectedAddress.Zip_Code);
//            //await TryModifyField("STREET", SelectedAddress.Street);
//            //await TryModifyField("EXTNUMBER", SelectedAddress.Ext_Number);
//            //await TryModifyField("INTNUMBER", SelectedAddress.Int_Number);

//            CanGetList = true;
//        }

//        private AddressData? GetSource()
//        {
//            if (IsCollection)
//            {
//                return BookingData.GetUnsharedElementValueAs<AddressData>(SectionData.Key, SectionData.Value.Keys.First(), ElementData.Element_ID);
//            }

//            return BookingData.GetSharedElementValueAs<AddressData>(ElementData.Element_ID);
//        }

//        protected bool GetElementReadonly()
//        {
//            if (BookingData.GetIsReadonly())
//            {
//                return true;
//            }
//            return ElementData.ReadOnly;
//        }

//        private IEnumerable<T> GetPreloadedListAs<T>(string field_name, bool override_check = false)
//        {
//            if (ElementData.Complex_Data[field_name].Contains_List && !ElementData.Complex_Data[field_name].Is_Autocomplete_List && (CanGetList || override_check) && ElementData.Complex_Data[field_name].Preloaded_List is not null)
//            {
//                return GetComplexPreloadedListAs<T>(field_name);
//            }
//            return Enumerable.Empty<T>();
//        }
//    }
//}
