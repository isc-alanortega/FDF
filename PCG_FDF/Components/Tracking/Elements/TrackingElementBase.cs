using Microsoft.AspNetCore.Components;
using MudBlazor.Services;
using Newtonsoft.Json;
using PCG_ENTITIES.PCG_FDF.BookingEntities;
using PCG_ENTITIES.PCG_FDF.TrackingEntities;
using PCG_FDF.Data.ComponentDI;
using PCG_FDF.Data.ComponentDI.Tracking;
using PCG_FDF.Data.Localization;

namespace PCG_FDF.Components.Tracking.Elements
{
    public class TrackingElementBase : ComponentBase, IDisposable
    {
        [Inject]
        protected GlobalBreakpointService BreakpointService { get; set; }
        [Inject]
        protected GlobalLocalizer Localize { get; set; }
        [Inject]
        protected TrackingDataCollection TrackingData { get; set; }

        [Parameter, EditorRequired]
        public TrackingElement ElementData { get; set; }

        [Parameter]
        public int IDStage { get; set; }

        [Parameter]
        public int IDService { get; set; }

        protected async override Task OnInitializedAsync()
        {
            await BreakpointService.InitializeService();
            await base.OnInitializedAsync();
        }

        protected override void OnInitialized()
        {
            BreakpointService.OnChange += StateHasChanged;
            TrackingData.OnChange += StateHasChanged;
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
            TrackingData.OnChange -= StateHasChanged;
            GC.SuppressFinalize(this);
        }

        protected T? DeserializeElementDataAs<T>()
        {
            return JsonConvert.DeserializeObject<T>(TrackingData.GetTrackingElementData().Element_Data);
        }

        public int GetElement_ID()
        {
            return TrackingData.GetTrackingElementData().Element_ID;
        }

        public int GetElement_Type()
        {
            return TrackingData.GetTrackingElementData().Element_Type;
        }

        public int GetElement_Sequence()
        {
            return TrackingData.GetTrackingElementData().Element_Sequence; 
        }
    }
}
