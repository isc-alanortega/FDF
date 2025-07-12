using MudBlazor;
using PCG_FDF.Data.Entities;

namespace PCG_FDF.Data.ComponentDI
{
    /// <summary>
    /// DI que se encarga de contener el estado de una fila del componente selector
    /// </summary>
    public class ComplexLevelService
    {
        private int chipsetLevel { get; set; }
        public Action<Tuple<ComplexService, int>> updateAction { get; set; }
        private ComplexService? associatedService { get; set; }
        private MudChip? selectedChip { get; set; }

        /// <summary>
        /// Obtiene el objeto MudChip que se seleccinó
        /// </summary>
        /// <returns>MudChip seleccionado</returns>
        public MudChip? GetSelectedChip()
        {
            return selectedChip;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        public void setLevel(int level)
        {
            chipsetLevel = level;
        }

        /// <summary>
        /// Elimina las referencias a objetos de este DI
        /// </summary>
        public void fullClear()
        {
            associatedService = null;
            selectedChip = null;
        }

        /// <summary>
        /// Recibe el MudChip seleccionado y cambia el servicio asociado con la selección de acuerdo a este
        /// También ejecuta la acción "updateAction" la cual realiza un cambio en el DI principal del componente
        /// </summary>
        /// <param name="chip">Objeto MudChip proporcionado por el componente "ComplexChipset" al haber un cambio en la selección</param>
        public void SetSelectedChip(MudChip? chip)
        {
            if (chip is not null)
            {
                associatedService = (ComplexService)chip.Tag;
                updateAction.Invoke(new Tuple<ComplexService, int>(associatedService, chipsetLevel));
            }
            else
            {
                updateAction.Invoke(new Tuple<ComplexService, int>(associatedService, chipsetLevel));
                associatedService = null;
            }
            selectedChip = chip;
        }

    }
}
