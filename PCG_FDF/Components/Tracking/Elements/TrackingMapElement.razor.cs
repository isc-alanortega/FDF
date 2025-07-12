using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PCG_ENTITIES.Enums;
using PCG_ENTITIES.PCG_FDF.UtilityEntities;

namespace PCG_FDF.Components.Tracking.Elements
{
    public partial class TrackingMapElement
    {
        #region INSTANCIAS JS
        private ElementReference MapElement;
        private IJSObjectReference? MapModule;
        private IJSObjectReference? MapInstance;
        #endregion

        /// <summary>
        /// Indice del tipo de busqueda tracking seleccionado
        /// </summary>
        private ETipoTracking SelectedTypeSearchTRacking { get; set; } = ETipoTracking.NA;
        /// <summary>
        /// Lista de unidades del tracking (todos los datos)
        /// </summary>
        private IList<TrackingUnit>? TrackingUnits { get; set; }
        /// <summary>
        /// Unidad seleccionada
        /// </summary>
        private TrackingUnit? SelectedUnit { get; set; }

        private int SelectedTimeLimit { get; set; } = 1;

        private bool AllUnits { get; set; } = false;
        private bool AllGPS { get; set; } = false;
        private bool Readonly { get; set; } = false;

        private Func<TrackingUnit, string> UnitConverter = unit => unit?.Placas ?? string.Empty;
        private Func<string?, string> IntConverter = value => value ?? string.Empty;
        private string? SelectedGPS { get; set; }

        #region FILTRO FECHA
        private DateTime DateFrom { get; set; } = DateTime.Now.AddDays(-1);
        private DateTime DateTo { get; set; } = DateTime.Now;

        private DateTime MinDate { get; set;  }
        private DateTime MaxDate { get; set; }
        #endregion

        #region FILTRO HORAS
        private TimeSpan TimeFrom { get; set; }
        private TimeSpan TimeTo { get; set; }
        #endregion

        #region LIFE CYCLE BALZOR
        protected async override Task OnInitializedAsync()
        {
            BreakpointService.OnChange += StateHasChanged;
            TrackingData.OnChange += StateHasChanged;
            await BreakpointService.InitializeService();
            TrackingData.RefreshMap = RefreshMapAsync;
            InitializePropertys();

            await base.OnInitializedAsync();
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await BreakpointService.ReCheckSize();
                StateHasChanged();
                await SetJSReferenceAsync();
                await AddTrunksToMapAsync();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private void InitializePropertys()
        {
            var dateFrom = TimeZoneInfo.ConvertTimeFromUtc(ElementData.DateFrom, TimeZoneInfo.Local);
            var dateTo = TimeZoneInfo.ConvertTimeFromUtc(ElementData.DateTo, TimeZoneInfo.Local);

            DateFrom = dateFrom;
            DateTo = dateTo;

            MinDate = DateFrom;
            MaxDate = DateTo;

            TimeFrom = new TimeSpan(DateFrom.Hour, DateFrom.Minute, DateFrom.Second);
            TimeTo = new TimeSpan(DateTo.Hour, DateTo.Minute, DateTo.Second);
        }

        public void Dispose()
        {
            BreakpointService.OnChange -= StateHasChanged;
            TrackingData.OnChange -= StateHasChanged;
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (MapInstance is not null)
            {
                await MapInstance.DisposeAsync();
            }

            if (MapModule is not null)
            {
                await MapModule.DisposeAsync();
            }
        }
        #endregion

        private async Task RefreshMapAsync()
        {
            if (MapModule is null || (TrackingUnits is null || !TrackingUnits.Any()))
            {
                await SetJSReferenceAsync();
            }

            TrackingUnits = DeserializeElementDataAs<IList<TrackingUnit>>();
            await AddTrunksToMapAsync();
            await UpdateTrackingDisplayAsync();
        }

        private async Task SetJSReferenceAsync()
        {
            TrackingUnits = DeserializeElementDataAs<IList<TrackingUnit>>();
            MapModule = await JS.InvokeAsync<IJSObjectReference>("import", "./scripts/map_helper.js");
            MapInstance = await MapModule.InvokeAsync<IJSObjectReference>("InitializeMap", MapElement);
        }

        private async Task AddTrunksToMapAsync()
        {
            if (TrackingUnits is not null)
            {
                foreach (var truck_data in TrackingUnits)
                {
                    await MapModule!.InvokeVoidAsync(
                        "AddTruckToMap",
                        truck_data.Unit_ID,
                        truck_data.Placas,
                        truck_data.GPS_Positions.ToDictionary(key => key.Key,
                            value => value.Value
                            .OrderBy(p => p.Date).Select(p => new object[] {
                                p.Latitude,
                                p.Longitude,
                                TimeZoneInfo.ConvertTimeFromUtc(p.Date, TimeZoneInfo.Local).ToString("dd/MM/yyyy hh:mm tt"),
                                p.Altitud,
                                p.Velocidad,
                                p.Angulo_Desplazamiento,
                                p.GPS_Name,
                                TimeZoneInfo.ConvertTimeFromUtc(p.Date, TimeZoneInfo.Local),
                            }).ToArray()),
                        MapInstance,
                        localize.Get("global_info_language")
                    );
                }
            }
        }

        #region FILTER TIME TRACKING
        private async Task HandleOnChangeTimeFromAsync(TimeSpan? value)
        {
            if (value.HasValue)
            {
                TimeFrom = value.Value;
                await UpdateTrackingDisplayAsync();
            }
        }

        private async Task HandleOnChangeTimeToAsync(TimeSpan? value)
        {
            if (value.HasValue)
            {
                TimeTo = value.Value;
                await UpdateTrackingDisplayAsync();
            }
        }

        private async Task SelecteTimeLimitChanged(int limit)
        {
            SelectedTimeLimit = limit;
            await UpdateTrackingDisplayAsync();
        }
        #endregion


        #region FILTER DATE TRACKING
        private async Task HandleChangedDateFromAsync(DateTime? value)
        {
            if (value.HasValue)
            {
                DateFrom = value.Value;
                await UpdateTrackingDisplayAsync();
            }
        }

        private async Task HandleChangedDateToAsync(DateTime? value)
        {
            if (value.HasValue)
            {
                DateTo = value.Value;


                await UpdateTrackingDisplayAsync();
            }
        }
        #endregion FILTER DATETIME


        #region TYPE FILTER TRAKING SELECTED (ShowAllGpsForAllTrucks/ShowAllGpsForTruck/ShowSingleGps)
        private async Task HandleChangedAllUnitsAsync(bool value)
        {
            SelectedTypeSearchTRacking = value ? ETipoTracking.AllGpsForAllTrucks : ETipoTracking.SingleGps;
            await UpdateTrackingDisplayAsync();
        }

        private async Task HandleChangedAllGPSAsync(bool value)
        {
            SelectedTypeSearchTRacking = value ? ETipoTracking.AllGpsForTruck : ETipoTracking.SingleGps;
            await UpdateTrackingDisplayAsync();
        }

        private async Task SelectedGPSChanged(string? GPS)
        {
            if (!string.IsNullOrEmpty(GPS))
            {
                SelectedGPS = GPS;
                SelectedTypeSearchTRacking = ETipoTracking.SingleGps;
                await UpdateTrackingDisplayAsync();
            }
        }

        private (DateTime dateFrom, DateTime dateTo) GetDateTimeFilter()
            => (
                new(DateFrom.Year, DateFrom.Month, DateFrom.Day, TimeFrom.Hours, TimeFrom.Minutes, TimeFrom.Seconds),
                new (DateTo.Year,  DateTo.Month,   DateTo.Day,   TimeTo.Hours,   TimeTo.Minutes,   TimeTo.Seconds)
            );

        private async Task UpdateTrackingDisplayAsync()
        {
            if (SelectedTypeSearchTRacking == ETipoTracking.NA)
            {
                return;
            }

            Readonly = SelectedTypeSearchTRacking != ETipoTracking.SingleGps;
            var (dateFrom, dateTo) = GetDateTimeFilter();

            switch (SelectedTypeSearchTRacking)
            {
                case ETipoTracking.AllGpsForAllTrucks:
                    AllUnits = true;
                    AllGPS = false;
                    await MapModule!.InvokeVoidAsync("ShowAllGpsForAllTrucks");
                    break;

                case ETipoTracking.SingleGps when SelectedUnit is not null && !string.IsNullOrEmpty(SelectedGPS):
                    AllUnits = false;
                    AllGPS = false;
                    await MapModule!.InvokeVoidAsync("ShowSingleGps", SelectedUnit.Unit_ID, Convert.ToInt32(SelectedGPS), dateFrom, dateTo, true);
                    break;

                case ETipoTracking.AllGpsForTruck when SelectedUnit is not null:
                    AllGPS = true;
                    AllUnits = false;
                    await MapModule!.InvokeVoidAsync("ShowAllGpsForTruck", SelectedUnit.Unit_ID, dateFrom, dateTo, true);
                    break;
                default:
                    AllGPS = false;
                    AllUnits = false;
                    SelectedTypeSearchTRacking = ETipoTracking.NA;
                    await MapModule!.InvokeVoidAsync("HideAllPolylines");
                    break;
            }
        }
        #endregion TYPE FILTER TRAKING SELECTED (ShowAllGpsForAllTrucks/ShowAllGpsForTruck/ShowSingleGps)


        private void SelectedUnitChanged(TrackingUnit? Unit)
        {
            SelectedUnit = Unit;
        }
    }
}
