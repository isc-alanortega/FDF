using PCG_ENTITIES.PCG;
using PCG_FDF.Data.Entities;
using PCG_FDF.Data.DataAccess;
using PCG_FDF.Utility;
using PCG_ENTITIES.Enums;
using PCG_ENTITIES.PCG_FDF.UtilityEntities;

namespace PCG_FDF.Data.ComponentDI
{
    /// <summary>
    /// DI que se encarga de contener el estado del componente selector de servicios
    /// </summary>
    public class ComplexTreeService
    {
        // Data fields
        // Fake service that acts as root for the first row of the selector
        private ComplexService ROOT;
        // Integer that keeps track of what the lowest level of the tree on display is
        private int currentTreeLevel;
        // Holds the DI of each row's Owned Service for cleanup
        private List<ComplexLevelService> subComponentStatus;
        // Holds a dictionary of the selected service at each level
        private IDictionary<int, ComplexService> CurrentSelectedTree;
        // Holds a dictionary of all services on display by ID
        private IDictionary<Guid, ComplexService> CurrentServices;
        private CategoriasFiltradas UnitCategoryInfo;
        private bool Should_Block { get; set; } = false;
        // Action that triggers a StateHasChanged event to rerender the component
        public event Action OnChange;
        private readonly PCG_FDF_DB DATA_ACCESS;
        private readonly WhiteLabelManager WhiteLabel;

        /// <summary>
        /// Constructor sin parámetros, inicializa el objeto con todos los servicios "padre"
        /// Y los almacena como hijos de "ROOT"
        /// </summary>
        public ComplexTreeService(PCG_FDF_DB database, WhiteLabelManager whiteLabel)
        {
            subComponentStatus = new List<ComplexLevelService>();
            ROOT = new ComplexService(-1, "", -1, "", "", null, null, "", null, -1, false, null, false, null, new Dictionary<Guid, ComplexService>(), new List<SubserviciosEntidad>());
            CurrentServices = new Dictionary<Guid, ComplexService>();
            CurrentSelectedTree = new Dictionary<int, ComplexService>();
            currentTreeLevel = 0;
            DATA_ACCESS = database;
            WhiteLabel = whiteLabel;
        }

        public async Task initTreeAsync(String lang)
        {
            IEnumerable<CategoriasEntidad> categoriasPadre;
            if (WhiteLabel.Current_Page != EPages.Cima_Group)
            {
                UnitCategoryInfo = await DATA_ACCESS.GetCategoriasFiltradas(WhiteLabel.GetPageID()!.Value);
                categoriasPadre = await DATA_ACCESS.GetCategoriasPrincipalesFiltradas(lang, string.Join(",", UnitCategoryInfo.CategoriasPrincipales));
            }
            else
            {
                categoriasPadre = await DATA_ACCESS.GetCategoriasPrincipales(lang);
            }
            foreach (CategoriasEntidad categoriaPadre in categoriasPadre)
            {
                ComplexService serviceFromRoot = new ComplexService(categoriaPadre.ID, categoriaPadre.Codigo, 0, categoriaPadre.Nombre_corto, categoriaPadre.Nombre, null, null, categoriaPadre.Icon, categoriaPadre.Viewbox, currentTreeLevel, false, categoriaPadre.Descripcion, false, null, new Dictionary<Guid, ComplexService>(), new List<SubserviciosEntidad>());
                ROOT.addChild(serviceFromRoot);
                CurrentServices.Add(serviceFromRoot.GUID, serviceFromRoot);
            }
            CurrentSelectedTree.Add(-1, ROOT);
        }

        /// <summary>
        /// Dispara el evento de StateHasChanged y notifica al componente principal
        /// </summary>
        private void NotifyStateChanged() => OnChange?.Invoke();

        public bool GetDisabled()
        {
            return Should_Block;
        }

        /// <summary>
        /// Construye el árbol del servicio que se ha seleccionado
        /// </summary>
        /// <param name="buildTarget">Entero que representa la llave asociada con el servicio que se ha seleccionado</param>
        private async Task treeBuilder(int buildTarget, Guid GUID, bool shouldSearch)
        {
            try
            {
                Should_Block = true;
                if (!shouldSearch)
                {
                    var categorias = await DATA_ACCESS.GetCategorias(LanguageUtil.getCurrentCulture(), buildTarget);
                    if (!categorias.Any())
                    {
                        var servicios = await DATA_ACCESS.GetServicios(LanguageUtil.getCurrentCulture(), buildTarget, WhiteLabel.GetPageID());
                        foreach (ServiciosCompletosEntidad servicio in servicios)
                        {
                            ComplexService newService = new ComplexService(servicio.Servicio.ID, servicio.Servicio.Codigo, servicio.Servicio.Version, servicio.Servicio.Nombre_corto, servicio.Servicio.Nombre, servicio.Servicio.Alias, servicio.Servicio.CalculoCantidad, servicio.Servicio.Icon, servicio.Servicio.Viewbox, currentTreeLevel + 1, true, servicio.Servicio.Descripcion, servicio.Servicio.PortCapital, CurrentServices[GUID], new Dictionary<Guid, ComplexService>(), servicio.Subservicios, servicio.Servicio.TipoClientePC, servicio.Servicio.NoDescargarBooking, servicio.Servicio.NoMostrarCotizacion, servicio.Servicio.RequireBillOfLanding);
                            CurrentServices[GUID].addChild(newService);
                            CurrentServices.Add(newService.GUID, newService);
                        }
                    }
                    else
                    {
                        List<CategoriasEntidad> filtered_categories = categorias.ToList();
                        if (WhiteLabel.Current_Page != EPages.Cima_Group)
                        {
                            filtered_categories.RemoveAll(category => !UnitCategoryInfo.Categorias.Contains(category.ID));
                        }
                        foreach (CategoriasEntidad categoria in filtered_categories)
                        {
                            ComplexService newService = new ComplexService(categoria.ID, categoria.Codigo, 0, categoria.Nombre_corto, categoria.Nombre, null, null, categoria.Icon, categoria.Viewbox, currentTreeLevel + 1, false, categoria.Descripcion, false, CurrentServices[GUID], new Dictionary<Guid, ComplexService>(), new List<SubserviciosEntidad>(), "N");
                            CurrentServices[GUID].addChild(newService);
                            CurrentServices.Add(newService.GUID, newService);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                Should_Block = false;
            }
        }

        /// <summary>
        /// Recorta el árbol hasta el nivel seleccionado
        /// </summary>
        /// <param name="serviceLevel">Entero que representa el nivel del servicio que se ha seleccionado</param>
        private void treePruner(int serviceLevel)
        {
            for (int i = currentTreeLevel - 1; i >= serviceLevel; i--)
            {
                foreach (Guid ID in CurrentSelectedTree[i].Children.Keys)
                {
                    CurrentServices.Remove(ID);
                }
                CurrentSelectedTree[i].Children.Clear();
                CurrentSelectedTree.Remove(i);
            }
        }

        /// <summary>
        /// Añade el servicio construido al diccionario que contiene los servicios seleccionados a cada nivel
        /// También añade que no se ha seleccionado nada en el nuevo nivel
        /// </summary>
        /// <param name="serviceIdentifier">Entero que representa la llave asociada con el servicio que se ha seleccionado</param>
        private void appendToTree(Guid serviceIdentifier)
        {
            CurrentSelectedTree.Add(currentTreeLevel, CurrentServices[serviceIdentifier]);
        }

        /// <summary>
        /// Se encarga de limpiar la selección actual del componente que se encuentra a un nivel inferior del servicio que se seleccionó
        /// </summary>
        /// <param name="serviceLevel">Nivel del servicio seleccionado</param>
        private void clearSubComponent(int serviceLevel)
        {
            var sub = subComponentStatus.ElementAt(serviceLevel + 1);
            if (sub is not null)
            {
                sub.fullClear();
            }
        }

        /// <summary>
        /// Método principal de la clase, se encarga de manejar todas las acciones al momento de seleccionar o deseleccionar servicios
        /// Es llamado automáticamente por medio de la acción "updateAction" en el DI del componente
        /// </summary>
        /// <param name="clickedService">Modelo del servicio particular al que se hizo click</param>
        /// <param name="serviceLevel">Entero que representa el nivel del árbol en el que se encuentra</param>
        private async Task onServiceClick(ComplexService clickedService, int serviceLevel)
        {
            int serviceIdentifier = clickedService.Id;
            // We're clicking an already selected service this means we should collapse the tree to the clicked chipsetLevel
            // Its necessary to "pop" every chipsetLevel up to "serviceLevel" out of the built tree along with the selected service's children
            if (CurrentSelectedTree.TryGetValue(serviceLevel, out _))
            {
                if (CurrentSelectedTree[serviceLevel].Id == serviceIdentifier)
                {
                    treePruner(serviceLevel);
                    currentTreeLevel = serviceLevel;
                }
                // We're clicking a service in the same chipsetLevel, however this is not the same service, but a different one
                // It's necessary to then swap the current chipsetLevel of the tree selection for the one that was just selected and render its children
                else
                {
                    treePruner(serviceLevel);
                    clearSubComponent(serviceLevel);
                    currentTreeLevel = serviceLevel;
                    await treeBuilder(serviceIdentifier, clickedService.GUID, clickedService.isActionable);
                    appendToTree(clickedService.GUID);
                    currentTreeLevel++;
                }
            }
            else
            {
                await treeBuilder(serviceIdentifier, clickedService.GUID, clickedService.isActionable);
                appendToTree(clickedService.GUID);
                currentTreeLevel++;
            }
        }

        /// <summary>
        /// Añade una referencia al DI del componente ComplexChipset que fué añadido
        /// Este método se ejecuta de manera automática por Blazor al lanzar el método OnInitialized
        /// Del componente, no debe servicioSeleccionado llamado de manera manual
        /// </summary>
        /// <param name="service">Objeto DI creado por el componente ComplexChipset al servicioSeleccionado inicializado</param>
        public void addSubState(ComplexLevelService service)
        {
            subComponentStatus.Add(service);
        }

        /// <summary>
        /// Elimina la referencia al DI del componente ComplexChipset que se está removiendo
        /// Este método se ejecuta de manera automática por Blazor al lanzar el método Dispose
        /// Del componente, no debe servicioSeleccionado llamado de manera manual
        /// </summary>
        /// <param name="service">Objeto DI creado por el componente ComplexChipset al servicioSeleccionado inicializado</param>
        public void removeSubState(ComplexLevelService service)
        {
            subComponentStatus.Remove(service);
        }

        /// <summary>
        /// Obtiene el diccionario que contiene los servicios seleccionados a cada nivel
        /// </summary>
        /// <returns>IDictionary que contiene los servicios seleccionados a cada nivel</returns>
        public IDictionary<int, ComplexService> getSelectedTree()
        {
            return CurrentSelectedTree;
        }

        /// <summary>
        /// Méodo ejecutado por una acción al cambiar el valor de la selección de "ComplexChipset"
        /// </summary>
        /// <param name="args">Tuple que contiene el modelo del servicio particular al que se hizo click
        /// Y un entero que representa el nivel del árbol en el que se encuentra</param>
        public async Task actionExec(Tuple<ComplexService, int> args)
        {
            await onServiceClick(args.Item1, args.Item2);
            NotifyStateChanged();
        }

        public void fullClear()
        {
            foreach (var item in subComponentStatus)
            {
                item.fullClear();
            }
            treePruner(0);
            subComponentStatus.Clear();
            CurrentSelectedTree.Clear();
            CurrentServices.Clear();
            ROOT.Children.Clear();
            currentTreeLevel = 0;
        }
    }
}
