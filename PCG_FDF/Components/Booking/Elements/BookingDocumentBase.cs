using Microsoft.AspNetCore.Components;
using PCG_ENTITIES.PCG_FDF.BookingEntities;
using PCG_FDF.Data.ComponentDI.Booking;
using PCG_FDF.Data.ComponentDI;
using PCG_FDF.Data.Localization;
using PCG_FDF.Data.Entities;
using PCG_ENTITIES.Enums;
using Microsoft.AspNetCore.Components.Forms;

namespace PCG_FDF.Components.Booking.Elements
{
    public abstract class BookingDocumentBase : ComponentBase, IDisposable
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
        [Parameter, EditorRequired]
        public DocumentInitializer DocumentData { get; set; }
        protected bool Document_Disabled { get; set; } = false;

        protected async override Task OnInitializedAsync()
        {
            BreakpointService.OnChange += StateHasChanged;
            await BreakpointService.InitializeService();
            Document_Disabled = GetSource().Document_Uploaded && (GetSource().Document_Status.HasValue && GetSource().Document_Status!.Value != EDocumentStatus.REJECTED);
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

        //protected T? GetMetadataSourceAs<T>()
        //{
        //    if (IsCollection)
        //    {

        //    } else
        //    {

        //    }
        //}

        protected FullDocumentData GetSource()
        {
            // Unshared
            if (IsCollection)
            {
                return BookingData.GetUnsharedDocument(SectionData.Key, SectionData.Value.Keys.First(), DocumentData.Document_Subtype_ID);
            }
            // Shared
            else
            {
                return BookingData.GetSharedDocument(DocumentData.Document_Subtype_ID);
            }
        }

        public void Dispose()
        {
            BreakpointService.OnChange -= StateHasChanged;
        }

        protected bool GetDocumentReadonly()
        {
            bool required_data = false;

            if (IsCollection)
            {
                required_data = DocumentData.Metadata.Where(metadata => metadata.Value.Required).Select(metadata => metadata.Key)
                                    .All(metadata_key =>
                                    {
                                        var document = BookingData.GetUnsharedMetadataStorage()[SectionData.Key][SectionData.Value.Keys.First()][DocumentData.Document_Subtype_ID];
                                        if (document is null || !document.Any())
                                        {
                                            return true;
                                        }
                                        return document[metadata_key] is not null;
                                    });
            }
            else
            {
                required_data = DocumentData.Metadata.Where(metadata => metadata.Value.Required).Select(metadata => metadata.Value.Metadata_ID)
                                    .All(metadata_key =>
                                    {
                                        var document = BookingData.GetSharedMetadataStorage()[DocumentData.Document_Subtype_ID];
                                        if (document is null || !document.Any())
                                        {
                                            return true;
                                        }
                                        return document[metadata_key] is not null;
                                    });
            }

            if (!required_data)
            {
                return true;
            }


            if (BookingData.GetIsReadonly())
            {
                bool reupload_enabled = false;
                if (IsCollection)
                {
                    var status = BookingData.GetUnsharedDocument(SectionData.Key, SectionData.Value.Keys.First(), DocumentData.Document_Subtype_ID).Document_Status;
                    reupload_enabled = status is not null && status.Value == EDocumentStatus.REJECTED;
                    return !reupload_enabled || Document_Disabled;
                }
                else
                {
                    var status = BookingData.GetSharedDocument(DocumentData.Document_Subtype_ID).Document_Status;
                    reupload_enabled = status is not null && status.Value == EDocumentStatus.REJECTED;
                    return !reupload_enabled || Document_Disabled;
                }
            }

            return DocumentData.ReadOnly || Document_Disabled;
        }
    }
}
