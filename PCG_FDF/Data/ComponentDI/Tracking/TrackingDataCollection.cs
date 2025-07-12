using MudBlazor;
using Newtonsoft.Json;
using PCG_ENTITIES.Enums;
using PCG_ENTITIES.PCG_FDF.TrackingEntities;
using PCG_ENTITIES.PCG_FDF.UtilityEntities;
using PCG_ENTITIES.Resultsets;
using PCG_FDF.Data.DataAccess;

namespace PCG_FDF.Data.ComponentDI.Tracking
{
    public class TrackingDataCollection
    {
        /// <summary>
        /// Action that triggers a StateHasChanged event to rerender the shared components
        /// </summary>
        public event Action OnChange;
        private Guid? Initial_Contract_UUID { get; set; }
        private MudChip? Selected_Contract_Chip { get; set; }
        private TrackingInitializer? Tracking_Data { get; set; }
        private CartTreeItemData? Selected_Cart_Item { get; set; }
        private IEnumerable<TrackingStage>? TrackingStages { get; set; }
        private TrackingElement? TrackingElementData { get; set; }
        public Func<Task> RefreshMap { get; set; }

        #region CONSTRUCTOR
        private readonly PCG_FDF_DB _dataAcces;

        public TrackingDataCollection(PCG_FDF_DB dataAcces) 
        {
            _dataAcces = dataAcces;
        }
        #endregion CONSTRUCTOR

        /// <summary>
        /// Triggers the StateHasChanged event on shared components to rerender themselves
        /// </summary>
        private void NotifyStateChanged() => OnChange?.Invoke();

        public void SetInitialContractUUID(Guid UUID)
        {
            if (!Initial_Contract_UUID.HasValue)
            {
                Initial_Contract_UUID = UUID;
            }
        }

        public IEnumerable<TrackingStage>? TryGetTrackingStages()
        {
            if (Selected_Cart_Item is null || Selected_Cart_Item.Is_Package)
            {
                return null;
            }

            if (GetSelectedContract()!.Service_Stages.TryGetValue(Selected_Cart_Item.Product_ID, out var stages))
            {
                return stages.Values;
            }

            return Enumerable.Empty<TrackingStage>();
        }

        public void SetSelectedCartItem(CartTreeItemData Item)
        {
            Selected_Cart_Item = Item;
        }

        public CartTreeItemData? GetSelectedCartItem()
        {
            return Selected_Cart_Item;
        }

        public HashSet<CartTreeItemData> GetSelectedContractCart()
        {
            return GetSelectedContract()!.Contract_Shopping_Cart;
        }

        private void UnselectTree(HashSet<CartTreeItemData> Data)
        {
            foreach (var item in Data)
            {
                item.Is_Selected = false;
                if (item.Has_Child)
                {
                    UnselectTree(item.Tree_Items);
                }
            }
        }

        public void SetSelectedContractChip(MudChip Contract_Chip)
        {
            if (Selected_Cart_Item is not null)
            {
                UnselectTree(GetSelectedContractCart());
            }
            Selected_Cart_Item = null;
            Selected_Contract_Chip = Contract_Chip;
        }

        public MudChip? GetSelectedContractChip()
        {
            return Selected_Contract_Chip;
        }

        public ContractInfo? GetSelectedContract()
        {
            return Selected_Contract_Chip is null ? null : Selected_Contract_Chip.Tag as ContractInfo;
        }

        public Guid? GetInitialContractUUID()
        {
            return Initial_Contract_UUID;
        }

        public void SetTrackingData(TrackingInitializer Data, bool refreshRender = true)
        {
            if (Tracking_Data is null || refreshRender == false)
            {
                Tracking_Data = Data;

                if (refreshRender)
                {
                    NotifyStateChanged();
                }
            }
        }

        public TrackingInitializer? GetTrackingData()
        {
            return Tracking_Data;
        }

        public void ResetSelf()
        {
            Initial_Contract_UUID = null;
            Tracking_Data = null;
            Selected_Contract_Chip = null;
            Selected_Cart_Item = null;
            TrackingStages = null;
            TrackingElementData = null; 
        }

        #region REFRESH TRACKING
        public async Task RefreshTrackingData(RequestTrackingForm request)
        {
            var trackingData = await _dataAcces.SendAuthTAsync<APIResult<TrackingInitializer?>?>("/PCG_FDFTracking/PostGetBookingTracking", HttpMethod.Post, null, JsonConvert.SerializeObject(request));

            if (trackingData is null || !trackingData.Operation_Succeeded)
            {
                return;
            }

            SetTrackingData(trackingData.Result!, false);

            if (GetSelectedCartItem() is not null)
            {
                var trackingStages = TryGetTrackingStages();
                SetTrackingStageParameter(trackingStages);
            }

            NotifyStateChanged();

            if (RefreshMap != null)
            {
                await RefreshMap();
            }

            NotifyStateChanged();
            return;
        }

        public void SetTrackingStageParameter(IEnumerable<TrackingStage> data)
        {
            TrackingStages = data;
        }

        public IEnumerable<TrackingStage> GetTrackingStageParameter()
        {
            return TrackingStages;
        }

        public void SetTrackingElementData(TrackingElement elementData)
        {
            TrackingElementData = elementData;
        }

        public TrackingElement GetTrackingElementData()
        {
            return TrackingElementData;
        }

        #endregion

        #region Colors
        public string GetMessageTypeClass(ETrackingMessageType? Type)
        {
            return Type switch
            {
                ETrackingMessageType.INFO => " color-info",
                ETrackingMessageType.WARNING => " color-warning",
                ETrackingMessageType.CRITICAL => " color-critical",
                _ => string.Empty,
            };
        }

        public Color GetMessageTypeColor(ETrackingMessageType? Type)
        {
            return Type switch
            {
                ETrackingMessageType.INFO => Color.Success,
                ETrackingMessageType.WARNING => Color.Warning,
                ETrackingMessageType.CRITICAL => Color.Error,
                _ => Color.Inherit,
            };
        }

        public Color GetMessageTypeColorText(ETrackingMessageType? Type)
        {
            return Type switch
            {
                ETrackingMessageType.INFO => Color.Success,
                ETrackingMessageType.WARNING => Color.Warning,
                ETrackingMessageType.CRITICAL => Color.Error,
                _ => Color.Inherit,
            };            
        }
        #endregion
    }
}
