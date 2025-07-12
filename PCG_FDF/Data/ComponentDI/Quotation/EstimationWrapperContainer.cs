using PCG_FDF.Data.Entities;

namespace PCG_FDF.Data.ComponentDI.Quotation
{
    public class EstimationWrapperContainer
    {
        private PaquetesCompletosEditable? paqueteSeleccionado;
        private IService? servicioSeleccionado;
        // Action that triggers a StateHasChanged event to rerender the component
        public event Action OnChange;

        /// <summary>
        /// Dispara el evento de StateHasChanged y notifica al componente principal
        /// </summary>
        private void NotifyStateChanged() => OnChange?.Invoke();

        public PaquetesCompletosEditable? obtenerPaquete()
        {
            return paqueteSeleccionado;
        }

        public IService? obtenerServicio()
        {
            return servicioSeleccionado;
        }

        public void seleccionarPaquete(PaquetesCompletosEditable paquete)
        {
            paqueteSeleccionado = paquete;
            servicioSeleccionado = null;
        }

        public void seleccionarServicio(IService servicio, bool IsPackage)
        {
            servicioSeleccionado = servicio;
            if (!IsPackage)
            {
                paqueteSeleccionado = null;
            }
            NotifyStateChanged();
        }
    }
}
