using Microsoft.AspNetCore.Components;
using PCG_FDF.Data.ComponentDI;
using PCG_FDF.Data.Localization;
using PCG_FDF.Data.ComponentDI.Quotation;
using PCG_ENTITIES.PCG_FDF.QuotationEntities;
using PCG_ENTITIES.Enums;
using PCG_FDF.Utility;

namespace PCG_FDF.Components.Quotation.Elements
{
    public class QuotationElementBase : ComponentBase, IDisposable
    {
        [Inject]
        protected GlobalBreakpointService Breakpoint_Service { get; set; }
        [Inject]
        protected QuotationDataCollection Quotation_Data { get; set; }
        [Inject]
        protected GlobalLocalizer Localize { get; set; }

        [CascadingParameter(Name = "CardData")]
        public Card Card { get; set; }
        [CascadingParameter(Name = "SingleElement")]
        public bool SingleElement { get; set; }

        [Parameter, EditorRequired]
        public Element ElementData { get; set; }

        protected async override Task OnInitializedAsync()
        {
            await Breakpoint_Service.InitializeService();
            await base.OnInitializedAsync();
        }

        protected override void OnInitialized()
        {
            Breakpoint_Service.OnChange += StateHasChanged;
            Quotation_Data.OnChange += StateHasChanged;
            base.OnInitialized();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await Breakpoint_Service.ReCheckSize();
                StateHasChanged();
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        public string GetElementName()
        {
            if (LanguageUtil.Language == ELanguage.SPANISH)
            {
                return ElementData.Element_Name_ES;
            }
            else
            {
                return ElementData.Element_Name_EN;
            }
        }

        public void Dispose()
        {
            Breakpoint_Service.OnChange -= StateHasChanged;
            Quotation_Data.OnChange -= StateHasChanged;
            GC.SuppressFinalize(this);
        }

        protected bool GetElementReadonly()
        {
            if (Quotation_Data.IsGloballyReadonly())
            {
                return true;
            }
            return ElementData.Readonly;
        }

        protected T GetPreloadedValueAs<T>()
        {
            return (T)ElementData.Preloaded_Value;
        }

        protected IEnumerable<T> GetPreloadedListAs<T>()
        {
            return ElementData.Preloaded_List.Select(obj => (T)obj);
        }
    }
}
