using Microsoft.AspNetCore.Components;
using MudBlazor;
using Newtonsoft.Json.Linq;
using PCG_ENTITIES.Enums;
using PCG_ENTITIES.PCG_FDF.BookingEntities;
using PCG_FDF.Data.ComponentDI;
using PCG_FDF.Data.ComponentDI.Booking;
using PCG_FDF.Data.Entities;
using PCG_FDF.Data.Localization;

namespace PCG_FDF.Components.Booking.Elements
{
    public abstract class BookingElementBase : ComponentBase, IDisposable
    {
        [Inject]
        protected GlobalBreakpointService BreakpointService { get; set; }
        [Inject]
        protected BookingDataCollection BookingData { get; set; }
        [Inject]
        protected GlobalLocalizer Localize { get; set; }
        [Inject]
        protected ISnackbar Snackbar { get; set; }

        [CascadingParameter(Name = "SectionForm")]
        public MudForm SectionForm { get; set; }
        [CascadingParameter(Name = "IsCollection")]
        public bool IsCollection { get; set; }
        [CascadingParameter(Name = "SectionData")]
        public KeyValuePair<int, IDictionary<Guid, StatefulSection>> SectionData { get; set; }
        [Parameter, EditorRequired]
        public Element ElementData { get; set; }

        protected async override Task OnInitializedAsync()
        {
            await BreakpointService.InitializeService();
            await base.OnInitializedAsync();
        }

        protected override void OnInitialized()
        {
            BreakpointService.OnChange += StateHasChanged;
            BookingData.OnChange += StateHasChanged;
            base.OnInitialized();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await BreakpointService.ReCheckSize();
                StateHasChanged();
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        public void Dispose()
        {
            BreakpointService.OnChange -= StateHasChanged;
            BookingData.OnChange -= StateHasChanged;
            GC.SuppressFinalize(this);
        }

        protected async Task SetComplexElementValid(bool Validation)
        {
            if (IsCollection)
            {
                BookingData.SetUnsharedComplexValid(Validation, SectionData.Key, SectionData.Value.First().Key, ElementData.Element_ID);
            }
            else
            {
                BookingData.SetSharedComplexValid(Validation, ElementData.Element_ID);
            }

            async Task<bool> GetIsValidForm()
            {
                await SectionForm.Validate();
                return SectionForm.IsValid;
            }
            bool isValid = BookingData.IsForceEdit ? true : await GetIsValidForm();
            SetValidSection(isValid);

        }

        private void SetValidSection(bool value)
        {
            bool validation_state = false;
            if (SectionData.Value.Values.First().Has_Chips)
            {
                var chip_elements = BookingData.TryGetSectionElements(SectionData.Key).Where(element => element.Type_ID == EElementType.OPTION_BOXES);
                // Unshared
                if (IsCollection)
                {
                    var validation = chip_elements.Select(element_data =>
                    {
                        var chip_data = BookingData.GetUnsharedStorage()[SectionData.Key][SectionData.Value.Keys.First()][element_data.Element_ID];
                        if (element_data.Required)
                        {
                            return chip_data != null;
                        }
                        else
                        {
                            return true;
                        }
                    });
                    validation_state = validation.All(valid => valid) && value;
                }
                // Shared
                else
                {
                    var validation = chip_elements.Select(element_data =>
                    {
                        var chip_data = BookingData.GetSharedStorage()[element_data.Element_ID];
                        if (element_data.Required)
                        {
                            return chip_data != null;
                        }
                        else
                        {
                            return true;
                        }
                    });
                    validation_state = validation.All(valid => valid) && value;
                }
            }
            else
            {
                validation_state = value;
            }

            if (IsCollection)
            {
                if (BookingData.GetUnsharedComplexValid().TryGetValue(SectionData.Key, out var sections))
                {
                    if (sections.TryGetValue(SectionData.Value.First().Key, out var elements))
                    {
                        var result = elements.Values.All(value => value);
                        validation_state = validation_state && value;
                    }
                }
            }
            else
            {
                var elements = BookingData.TryGetSectionElements(SectionData.Key);
                foreach (var element in elements)
                {
                    if (BookingData.GetSharedComplexValid().TryGetValue(element.Element_ID, out var valid))
                    {
                        validation_state = validation_state && valid;
                    }
                }
            }

            SectionData.Value.First().Value.SetValidationState(validation_state);

            BookingData.UpdateGlobalValidationStatus();
        }
        protected bool GetElementReadonly()
        {
            if (BookingData.GetIsReadonly())
            {
                return true;
            }
            return ElementData.ReadOnly;
        }

        protected bool GetComplexMetadataElementReadonly(bool Document_Readonly)
        {
            if (BookingData.GetIsReadonly())
            {
                return Document_Readonly;
            }
            return ElementData.ReadOnly;
        }

        protected T GetPreloadedValueAs<T>()
        {
            return (T)ElementData.Preloaded_Value;
        }

        protected IEnumerable<T> GetPreloadedListAs<T>()
        {
            return ElementData.Preloaded_List.Select(obj => (T)obj);
        }

        protected IEnumerable<T> GetComplexPreloadedListAs<T>(string field_name)
        {
            return ElementData.Complex_Data[field_name].Preloaded_List.Select(obj => (T)obj);
        }

        protected void ShowError(string Error_Message)
        {
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomLeft;
            Snackbar.Add(Localize.Get(Error_Message), Severity.Error);
        }

    }
}
