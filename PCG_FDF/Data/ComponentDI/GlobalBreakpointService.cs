using MudBlazor;
using MudBlazor.Services;
using PCG_FDF.Utility;

namespace PCG_FDF.Data.ComponentDI
{
    public class GlobalBreakpointService
    {
        private Guid _subscriptionId;
        private Breakpoint _breakpoint { get; set; }

        private bool IsMobile = false;
        private bool IsMedium = false;
        private bool IsSmall = false;
        private bool IsExtraSmall = false;
        private bool Initialized = false;
        private readonly IBrowserViewportService BreakpointListener;
        // Action that triggers a StateHasChanged event to rerender the component
        public event Action OnChange;

        /// <summary>
        /// Dispara el evento de StateHasChanged y notifica al componente principal
        /// </summary>
        private void NotifyStateChanged() => OnChange?.Invoke();

        public GlobalBreakpointService(IBrowserViewportService breakpointListener) {
            BreakpointListener = breakpointListener;
        }

        public async Task InitializeService() {
            if (!Initialized) {
                //var subscriptionResult = await BreakpointListener.SubscribeAsync((breakpoint) =>
                //{
                //    _breakpoint = breakpoint;
                //    SetIsMobile(ResponsiveUtil.BreakpointCheck(_breakpoint));
                //    NotifyStateChanged();
                //}, new ResizeOptions
                //{
                //    ReportRate = 500,
                //    NotifyOnBreakpointOnly = true,
                //});

                _subscriptionId = Guid.NewGuid();

                await BreakpointListener.SubscribeAsync(_subscriptionId, (breakpoint) =>
                {
                    _breakpoint = breakpoint.Breakpoint;
                    SetIsMobile(ResponsiveUtil.BreakpointCheck(_breakpoint));
                    NotifyStateChanged();
                }, new ResizeOptions
                {
                    ReportRate = 500,
                    NotifyOnBreakpointOnly = true,
                });

                Initialized = true;
            }
        }

        public async Task ReCheckSize() {
            // Fix for bad Breakpoint Listener service
            // Initially always grabs Xs with normal initialization (see OnInitialized)
            _breakpoint = await BreakpointListener.GetCurrentBreakpointAsync();
            SetIsMobile(ResponsiveUtil.BreakpointCheck(_breakpoint));
        }

        private void SetIsMobile(ValueTuple<bool, bool, bool, bool> data) {
            IsMobile = data.Item1;
            IsMedium = data.Item2;
            IsSmall = data.Item3;
            IsExtraSmall = data.Item4;
        }

        /// <summary>
        /// Is Mobil when IsMedium || IsSmall || IsExtraSmall = true
        /// </summary>
        /// <returns><bool/returns>
        public bool GetIsMobile() {
            return IsMobile;
        }

        /// <summary>
        /// size >= 960px && <1280px
        /// </summary>
        /// <returns>bool</returns>
        public bool GetIsMedium()
        {
            return IsMedium;
        }

        /// <summary>
        /// size >=600px && <960px
        /// </summary>
        /// <returns>bool</returns>
        public bool GetIsSmall()
        {
            return IsSmall;
        }

        /// <summary>
        /// size <600px
        /// </summary>
        /// <returns>bool</returns>
        public bool GetIsExtraSmall()
        {
            return IsExtraSmall;
        }

        /// <summary>
        /// Retun Sm/Xs/Md/Lg
        /// </summary>
        /// <returns>string size</returns>
        public string GetStringSize()
        {
            return Enum.GetName(typeof(Breakpoint), _breakpoint);
        }

        /// <summary>
        /// Get actual enum size
        /// </summary>
        /// <returns>Breakpoint enum</returns>
        public Breakpoint GetEnumSize()
        {
            return _breakpoint;
        }

        public bool GetIsXsSmScreen()
        {
            return IsExtraSmall || IsSmall;
        }
    }
}
