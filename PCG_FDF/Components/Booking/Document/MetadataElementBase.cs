using Microsoft.AspNetCore.Components;
using PCG_ENTITIES.PCG_FDF.BookingEntities;
using PCG_FDF.Data.ComponentDI.Booking;
using PCG_FDF.Data.ComponentDI;
using PCG_FDF.Data.Localization;
using PCG_FDF.Data.Entities;

namespace PCG_FDF.Components.Booking.Document
{
    public abstract class MetadataElementBase : ComponentBase, IDisposable
    {
        [Inject]
        protected GlobalBreakpointService BreakpointService { get; set; }
        [Inject]
        protected BookingDataCollection BookingData { get; set; }
        [Inject]
        protected GlobalLocalizer Localize { get; set; }

        [CascadingParameter(Name = "IsCollection")]
        public bool IsCollection { get; set; }
        [CascadingParameter(Name = "SectionData")]
        public KeyValuePair<int, IDictionary<Guid, StatefulSection>> SectionData { get; set; }
        [CascadingParameter(Name = "DocumentData")]
        public DocumentInitializer DocumentData { get; set; }
        [CascadingParameter(Name = "DocumentDisabled")]
        public bool DocumentDisabled { get; set; }
        [Parameter, EditorRequired]
        public MetadataInitializer MetadataData { get; set; }

        protected async override Task OnInitializedAsync()
        {
            BreakpointService.OnChange += StateHasChanged;
            await BreakpointService.InitializeService();
            await base.OnInitializedAsync();
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
        }

        protected bool GetElementReadonly()
        {
            if (BookingData.GetIsReadonly())
            {
                return DocumentDisabled;
            }

            return BookingData.GetIsReadonly() || DocumentDisabled;
        }
    }
}
