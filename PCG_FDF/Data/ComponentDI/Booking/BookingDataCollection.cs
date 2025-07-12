namespace PCG_FDF.Data.ComponentDI.Booking
{
    using MudBlazor;
    using Newtonsoft.Json;
    using PCG_ENTITIES.Enums;
    using PCG_ENTITIES.PCG_FDF.BookingEntities;
    using PCG_ENTITIES.PCG_FDF.ContractSettigs;
    using PCG_ENTITIES.PCG_FDF.PortCapital;
    using PCG_ENTITIES.PCG_FDF.UtilityEntities;
    using PCG_FDF.Data.DataAccess;
    using PCG_FDF.Data.Entities;
    using PCG_FDF.Data.Localization;
    using PCG_FDF.Utility;
    using System;

    public class BookingDataCollection : IBooking
    {
        /// <summary>
        /// GUID de la contratación actual
        /// </summary>
        public Guid UUID { get; set; }
        /// <summary>
        /// Indicates the page this quotation was made from
        /// </summary>
        private EPages Booking_Page { get; set; }
        /// <summary>
        /// Indicates the page this quotation was made from
        /// </summary>
        private string Booking_URI { get; set; }
        /// <summary>
        /// ID del usuario que hizo la contratación
        /// </summary>
        public int User_ID { get; set; }
        /// <summary>
        /// ID del cliente que hizo la contratación
        /// </summary>
        public int Client_ID { get; set; }
        /// <summary>
        /// Indica si ya hay PDF disponible
        /// </summary>
        public bool PDF_Ready { get; set; } = false;
        /// <summary>
        /// Indica si se está contratando una cotización
        /// </summary>
        public bool Contracting { get; set; } = false;
        /// <summary>
        /// Usado solamente para inicialización posterior
        /// </summary>
        private EStatusContratacion? Status { get; set; }
        /// <summary>
        /// Folio de la contratación actual
        /// </summary>
        public string Invoice { get; set; }
        /// <summary>
        /// Indica si el formulario se encuentra en proceso de guardar
        /// </summary>
        private bool Is_Saving { get; set; } = false;
        /// <summary>
        /// Indica si el formulario ha sido guardado exitosamente
        /// </summary>
        private bool Has_Saved { get; set; } = false;
        /// <summary>
        /// Identifica el lenguaje en el que la contratación actual se está llevando a cabo
        /// </summary>
        private string Booking_Data_Language { get; set; }
        /// <summary>
        /// Diccionario que almacena la información de las secciones de la contratación actual
        /// </summary>
        private IDictionary<int, IDictionary<Guid, StatefulSection>> Sections { get; set; }
        /// <summary>
        /// Diccionario que almacena la información de los elementos de la contratación actual
        /// </summary>
        private IDictionary<int, Element> Elements { get; set; }
        /// <summary>
        /// Diccionario que almacena la información de los documentos de la contratación actual
        /// </summary>
        private IDictionary<int, DocumentInitializer> Documents { get; set; }
        /// <summary>
        /// Diccionario que relaciona una sección con sus elementos
        /// </summary>
        private IDictionary<int, HashSet<int>> Section_Relationship_Elements { get; set; }
        /// <summary>
        /// Diccionario que relaciona una sección con sus documentos
        /// </summary>
        private IDictionary<int, HashSet<int>> Section_Relationship_Documents { get; set; }
        /// <summary>
        /// Diccionario que guarda los valores de elementos compartidos
        /// </summary>
        private IDictionary<int, object?> Shared_Element_Data_Storage { get; set; } = new Dictionary<int, object?>();
        /// <summary>
        /// Diccionario que guarda los valores de documentos compartidos
        /// </summary>
        private IDictionary<int, FullDocumentData> Shared_Document_Data_Storage { get; set; } = new Dictionary<int, FullDocumentData>();
        /// <summary>
        /// Diccionario que guarda los valores de elementos de tipo OptionBox compartidos
        /// </summary>
        private IDictionary<int, MudChip?> Shared_Chip_Data_Storage { get; set; } = new Dictionary<int, MudChip?>();
        /// <summary>
        /// Diccionario que almacena el estado de validez de los elementos complejos
        /// </summary>
        private IDictionary<int, IDictionary<Guid, IDictionary<int, bool>>> Unshared_Complex_Element_Valid { get; set; } = new Dictionary<int, IDictionary<Guid, IDictionary<int, bool>>>();
        /// <summary>
        /// Diccionario que almacena el estado de validez de los elementos complejos
        /// </summary>
        private IDictionary<int, bool> Shared_Complex_Element_Valid { get; set; } = new Dictionary<int, bool>();
        /// <summary>
        /// Diccionario que guarda los valores de metadatos de documentos compartidos
        /// </summary>
        private IDictionary<int, IDictionary<int, object?>> Shared_Metadata_Storage { get; set; } = new Dictionary<int, IDictionary<int, object?>>();
        /// <summary>
        /// Diccionario que guarda los valores de documentos no compartidos
        /// </summary>
        private IDictionary<int, IDictionary<Guid, IDictionary<int, FullDocumentData>>> Unshared_Document_Data_Storage { get; set; } = new Dictionary<int, IDictionary<Guid, IDictionary<int, FullDocumentData>>>();
        /// <summary>
        /// Diccionario que guarda los valores de elementos no compartidos
        /// </summary>
        private IDictionary<int, IDictionary<Guid, IDictionary<int, object?>>> Unshared_Element_Data_Storage { get; set; } = new Dictionary<int, IDictionary<Guid, IDictionary<int, object?>>>();
        /// <summary>
        /// Diccionario que guarda los valores de elementos de tipo OptionBox no compartidos
        /// </summary>
        private IDictionary<int, IDictionary<Guid, IDictionary<int, MudChip?>>> Unshared_Chip_Data_Storage { get; set; } = new Dictionary<int, IDictionary<Guid, IDictionary<int, MudChip?>>>();
        /// <summary>
        /// Diccionario que guarda los valores de metadatos de documentos no compartidos
        /// </summary>
        private IDictionary<int, IDictionary<Guid, IDictionary<int, IDictionary<int, object?>>>> Unshared_Metadata_Storage { get; set; } = new Dictionary<int, IDictionary<Guid, IDictionary<int, IDictionary<int, object?>>>>();
        /// <summary>
        /// Diccionario que guarda los nombres de los servicios
        /// </summary>
        private IDictionary<int, string> Services { get; set; }
        /// <summary>
        /// Indica si el formulario ha sido inicializado completamente
        /// </summary>
        private bool Initialized { get; set; } = false;
        /// <summary>
        /// Indica si el formulario es de solo lectura
        /// </summary>
        private bool Readonly { get; set; } = false;
        /// <summary>
        /// Indica si los datos de la contratación han sido llenados de forma completa
        /// </summary>
        private bool Booking_Data_Complete { get; set; } = false;

        private bool ShowModal { get; set; } = false;

        #region PORT CAPITAL
        public bool IsPortCapital { get; set; } = false;

        public PortCapitalQuotationResponse? QuotationPortCapital { set; get; } = null;

        /// <summary>
        /// Indica si el pcb fue guardado exitosamente 
        /// </summary>
        private bool IsPCBSaving { get; set; } = false;

        private PortCapitalResponse<PortCapitalBookingResponse?> Pcb { get; set; } = null;

        private InstallmentInterests InstallmentSelected { get; set; }
        #endregion

        private bool IsServiceNoGenerateBooking { get; set; } = false;
        public  QuotationServicesSettigs ServiceSettings { get; set; }

        public bool IsForceEdit { get; set; } = false;


        /// <summary>
        /// Action that triggers a StateHasChanged event to rerender the shared components
        /// </summary>
        public event Action OnChange;

        /// <summary>
        /// DI to make Web API Calls
        /// </summary>
        private readonly PCG_FDF_DB DATA_ACCESS;
        private readonly WhiteLabelManager WhiteLabel;
        private readonly ISnackbar _snackbarServices;
        private readonly GlobalLocalizer _localizeService;


        /// <summary>
        /// DI Constructor
        /// </summary>
        /// <param name="_DATA_ACCESS">API DI</param>
        public BookingDataCollection(
            PCG_FDF_DB _DATA_ACCESS,
            WhiteLabelManager whiteLabel,
            ISnackbar snackbarServices,
            GlobalLocalizer localizeService
        )
        {
            DATA_ACCESS = _DATA_ACCESS;
            WhiteLabel = whiteLabel;
            _snackbarServices = snackbarServices;
            _localizeService = localizeService;
        }

        /// <summary>
        /// Método que se encarga de inicializar la colección de datos del formulario de contratación
        /// </summary>
        /// <param name="initialization_data">Datos de inicialización obtenidos de la Web API</param>
        public bool IsShowModal() => ShowModal;

        public void Initialize(BookingInitializer initialization_data, int user_id, int client_id, QuotationServicesSettigs servicesSettigs )
        {
            if (!Initialized)
            {
                FullJSONDeserializer.RegenerateObjectData(initialization_data);
                UUID = initialization_data.UUID;
                Booking_Page = WhiteLabel.Current_Page;
                Booking_URI = WhiteLabel.GetPageURI();
                User_ID = user_id;
                Client_ID = client_id;
                Booking_Data_Language = LanguageUtil.getCurrentCulture();
                Invoice = initialization_data.Invoice;
                Elements = initialization_data.Elements;
                Documents = initialization_data.Documents;
                Section_Relationship_Elements = initialization_data.Elements_Relationship_Sections;
                Section_Relationship_Documents = initialization_data.Documents_Relationship_Sections;
                Services = initialization_data.Services;
                Sections = initialization_data.Sections.ToDictionary(key => key.Key, value => (IDictionary<Guid, StatefulSection>)value.Value.ToDictionary(key => key.Key, value => new StatefulSection(value.Value)));
                ServiceSettings = servicesSettigs;

                // Shared
                foreach (var section in Sections.Where(sections => !sections.Value.Values.First().Multiply_By_Quantity).Select(section => section.Key))
                {
                    if (Section_Relationship_Elements.ContainsKey(section))
                    {
                        foreach (var element in Section_Relationship_Elements[section])
                        {
                            if (!Shared_Element_Data_Storage.ContainsKey(element))
                            {
                                Shared_Element_Data_Storage[element] = null;
                            }
                        }
                    }

                    if (Section_Relationship_Documents.ContainsKey(section))
                    {
                        foreach (var document in Section_Relationship_Documents[section])
                        {
                            if (!Shared_Metadata_Storage.ContainsKey(document))
                            {
                                Shared_Metadata_Storage[document] = Documents[document].Metadata.Keys.ToDictionary(key => key, value => null as object);
                                Shared_Document_Data_Storage[document] = new FullDocumentData()
                                {
                                    Document_Uploaded = false,
                                    Document_File = null,
                                    Uploaded_Document_ID = null,
                                    Document_Rejection_Reason = null,
                                    Document_Status = null
                                };
                            }
                        }
                    }
                }

                // Unshared
                foreach (var section in Sections.Where(sections => sections.Value.Values.First().Multiply_By_Quantity).Select(grouping => { return (SectionID: grouping.Key, SectionUUID: grouping.Value.Keys); }))
                {
                    Unshared_Element_Data_Storage[section.SectionID] = new Dictionary<Guid, IDictionary<int, object?>>();
                    Unshared_Document_Data_Storage[section.SectionID] = new Dictionary<Guid, IDictionary<int, FullDocumentData>>();
                    Unshared_Metadata_Storage[section.SectionID] = new Dictionary<Guid, IDictionary<int, IDictionary<int, object?>>>();
                    foreach (var section_UUID in section.SectionUUID)
                    {
                        if (Section_Relationship_Elements.ContainsKey(section.SectionID))
                        {
                            Unshared_Element_Data_Storage[section.SectionID][section_UUID] = Section_Relationship_Elements[section.SectionID].ToDictionary(key => key, value => null as object);
                        }
                        if (Section_Relationship_Documents.ContainsKey(section.SectionID))
                        {
                            Unshared_Metadata_Storage[section.SectionID][section_UUID] = Section_Relationship_Documents[section.SectionID].ToDictionary(key => key,
                            value => Documents[value].Metadata.Keys.ToDictionary(key => key, value => null as object) as IDictionary<int, object?>);
                            Unshared_Document_Data_Storage[section.SectionID][section_UUID] = Section_Relationship_Documents[section.SectionID].ToDictionary(key => key,
                            value => new FullDocumentData()
                            {
                                Document_Uploaded = false,
                                Document_File = null,
                                Uploaded_Document_ID = null,
                                Document_Rejection_Reason = null,
                                Document_Status = null
                            });
                        }
                    }
                }
                Initialized = true;
            }
        }

        public void MiddlewareInitialize(BookingMiddleware middleware)
        {
            if (!Initialized)
            {
                FullJSONDeserializer.RegenerateObjectData(middleware);

                UUID = middleware.GUID_Contratacion;
                User_ID = middleware.User_ID;
                Booking_Page = middleware.Booking_Page;
                Booking_URI = middleware.Booking_URI;
                Client_ID = middleware.Client_ID;
                Invoice = middleware.Folio;
                Booking_Data_Language = middleware.Booking_Data_Language;
                Sections = middleware.Sections.ToDictionary(key => key.Key, value => (IDictionary<Guid, StatefulSection>)value.Value.ToDictionary(key => key.Key, value => new StatefulSection(value.Value)));
                Elements = middleware.Elements;
                Documents = middleware.Documents;
                Section_Relationship_Elements = middleware.Section_Relationship_Elements;
                Section_Relationship_Documents = middleware.Section_Relationship_Documents;
                Shared_Element_Data_Storage = middleware.Shared_Element_Data_Storage;
                Shared_Document_Data_Storage = middleware.Shared_Document_Data_Storage.ToDictionary(key => key.Key, value => new FullDocumentData()
                {
                    Document_Uploaded = value.Value.Document_Uploaded,
                    Document_File = value.Value.Document_Uploaded ? new PlaceholderFile() : null,
                    Uploaded_Document_ID = value.Value.Uploaded_Document_ID,
                    Document_Rejection_Reason = value.Value.Document_Rejection_Reason,
                    Document_Status = value.Value.Document_Status
                });
                Unshared_Complex_Element_Valid = middleware.Unshared_Complex_Element_Valid;
                Shared_Complex_Element_Valid = middleware.Shared_Complex_Element_Valid;
                Shared_Metadata_Storage = middleware.Shared_Metadata_Storage;
                Unshared_Element_Data_Storage = middleware.Unshared_Element_Data_Storage;
                Unshared_Document_Data_Storage = middleware.Unshared_Document_Data_Storage.ToDictionary(key => key.Key, value =>
                (IDictionary<Guid, IDictionary<int, FullDocumentData>>)value.Value.ToDictionary(key => key.Key, value =>
                (IDictionary<int, FullDocumentData>)value.Value.ToDictionary(key => key.Key, value => new FullDocumentData()
                {
                    Document_Uploaded = value.Value.Document_Uploaded,
                    Document_File = value.Value.Document_Uploaded ? new PlaceholderFile() : null,
                    Uploaded_Document_ID = value.Value.Uploaded_Document_ID,
                    Document_Rejection_Reason = value.Value.Document_Rejection_Reason,
                    Document_Status = value.Value.Document_Status
                })));
                Unshared_Metadata_Storage = middleware.Unshared_Metadata_Storage;
                Services = middleware.Services;
                Readonly = middleware.Readonly;
                Booking_Data_Complete = middleware.Booking_Data_Complete;
                Initialized = true;
                ServiceSettings = middleware.ServicesSettigs;
                if (Readonly && Booking_Data_Complete)
                {
                    PDF_Ready = true;
                }

                if (IsForceEdit) ForceEditConract();
            }
        }

        private void ForceEditConract()
        {
            Readonly = false;
            PDF_Ready = false;
            Is_Saving = false;
        }

        /// <summary>
        /// Triggers the StateHasChanged event on shared components to rerender themselves
        /// </summary>
        private void NotifyStateChanged() => OnChange?.Invoke();

        /// <summary>
        /// DI Reset Switch
        /// </summary>
        public void ResetSelf()
        {
            UUID = Guid.Empty;
            Status = null;
            Invoice = string.Empty;
            Contracting = false;
            Is_Saving = false;
            Has_Saved = false;
            PDF_Ready = false;
            Booking_Data_Language = string.Empty;
            Sections?.Clear();
            Elements?.Clear();
            Documents?.Clear();
            Section_Relationship_Elements?.Clear();
            Section_Relationship_Documents?.Clear();
            Shared_Element_Data_Storage?.Clear();
            Shared_Document_Data_Storage?.Clear();
            Shared_Chip_Data_Storage?.Clear();
            Shared_Metadata_Storage?.Clear();
            Unshared_Complex_Element_Valid?.Clear();
            Unshared_Element_Data_Storage?.Clear();
            Unshared_Chip_Data_Storage?.Clear();
            Unshared_Metadata_Storage?.Clear();
            Services?.Clear();
            Initialized = false;
            Readonly = false;
            Booking_Data_Complete = false;

            // Propiedades para Port Capital;
            IsPortCapital = false;
            QuotationPortCapital = null;
            IsPCBSaving = false;
            Pcb = null;
            InstallmentSelected = null;
            IsForceEdit = false;
        }

        /// <summary>
        /// Obtiene si el formulario de contratación ya se encuentra inicializado
        /// </summary>
        /// <returns>Bool que indica el estado de inicialización del formulario de contratación</returns>
        public bool GetFormInitialized() => Initialized;

        public bool GetBookingHasSaved() => Has_Saved;

        public void SetStatus(EStatusContratacion status) => Status ??= status;

        public EStatusContratacion? GetStatus() => Status;

        /// <summary>
        /// Obtiene las secciones del formulario
        /// </summary>
        /// <returns>Diccionario que contiene todas las secciones del formulario</returns>
        public IDictionary<int, IDictionary<Guid, StatefulSection>> GetFormSections() => Sections;

        public IDictionary<int, string> GetServices() => Services;

        public string GetServiceName(int ServiceID) => Services[ServiceID];

        public string GetBookingLanguage() => Booking_Data_Language;

        public IDictionary<int, Element> GetElements() => Elements;

        public string GetBookingURI() => Booking_URI;

        public EPages GetBookingPage() => Booking_Page;

        public IDictionary<int, DocumentInitializer> GetDocuments() => Documents;

        public IDictionary<int, HashSet<int>> GetElementRelationships() => Section_Relationship_Elements;

        public IDictionary<int, HashSet<int>> GetDocumentRelationships() => Section_Relationship_Documents;

        public IEnumerable<Element> TryGetSectionElements(int SectionID)
            => (Section_Relationship_Elements.ContainsKey(SectionID))
                ? Section_Relationship_Elements[SectionID].Select(element_id => Elements[element_id])
                : Enumerable.Empty<Element>();

        public IEnumerable<DocumentInitializer> TryGetSectionDocuments(int SectionID)
            => (Section_Relationship_Documents.ContainsKey(SectionID)) 
                    ? Section_Relationship_Documents[SectionID].Select(document_id => Documents[document_id])
                    : Enumerable.Empty<DocumentInitializer>().ToHashSet();

        public T? GetSharedElementValueAs<T>(int Element_ID)
            =>(T?)Shared_Element_Data_Storage[Element_ID];

        public T? GetSharedMetadataValueAs<T>(int Document_ID, int Metadata_ID)
            => (T?)Shared_Metadata_Storage[Document_ID][Metadata_ID];

        public T? GetUnsharedMetadataValueAs<T>(int Section_ID, Guid Inner_Section_ID, int Document_ID, int Metadata_ID)
            => (T?)Unshared_Metadata_Storage[Section_ID][Inner_Section_ID][Document_ID][Metadata_ID];

        public T? GetUnsharedElementValueAs<T>(int Section_ID, Guid Inner_Section_ID, int Element_ID)
            =>  (T?)Unshared_Element_Data_Storage[Section_ID][Inner_Section_ID][Element_ID];
        
        public bool GetIsReadonly() => Readonly;

        public async Task WriteSharedElementValue(object? value, int elementId)
        {
            Shared_Element_Data_Storage[elementId] = value;
            await SaveBookingData();
        }

        public async Task WriteSharedMetadataValue(object? value, int documentId, int metadataId)
        {
            Shared_Metadata_Storage[documentId][metadataId] = value;
            await SaveBookingData();
        }

        public async Task WriteUnsharedMetadataValue(object? value, int sectionId, Guid innerSectionId, int documentId, int metadataId)
        {
            Unshared_Metadata_Storage[sectionId][innerSectionId][documentId][metadataId] = value;
            await SaveBookingData();
        }

        public async Task WriteUnsharedElementValue(object? value, int sectionId, Guid innerSectionId, int elementId)
        {
            Unshared_Element_Data_Storage[sectionId][innerSectionId][elementId] = value;
            await SaveBookingData();
        }

        public void TryBindSharedChipset(int elementId)
        {
            if (!Shared_Chip_Data_Storage.ContainsKey(elementId))
            {
                Shared_Chip_Data_Storage[elementId] = null;
            }
        }

        public void TryBindUnsharedChipset(int sectionId, Guid innerSectionId, int elementId)
        {
            if (!Unshared_Chip_Data_Storage.ContainsKey(sectionId))
            {
                Unshared_Chip_Data_Storage[sectionId] = new Dictionary<Guid, IDictionary<int, MudChip?>>();
                Unshared_Chip_Data_Storage[sectionId][innerSectionId] = new Dictionary<int, MudChip?>();
                Unshared_Chip_Data_Storage[sectionId][innerSectionId][elementId] = null;
            }
            else if (!Unshared_Chip_Data_Storage[sectionId].ContainsKey(innerSectionId))
            {
                Unshared_Chip_Data_Storage[sectionId][innerSectionId] = new Dictionary<int, MudChip?>();
                Unshared_Chip_Data_Storage[sectionId][innerSectionId][elementId] = null;
            }
            else if (!Unshared_Chip_Data_Storage[sectionId][innerSectionId].ContainsKey(elementId))
            {
                Unshared_Chip_Data_Storage[sectionId][innerSectionId][elementId] = null;
            }
        }

        public async Task<Stream?> GetBookingPDF()
            => await DATA_ACCESS.PostDownloadBookingPDF(UUID);

        private async Task SaveBookingData()
        {
            if (!Is_Saving)
            {
                Is_Saving = true;
                NotifyStateChanged();
                var save_result = await DATA_ACCESS.PostSaveBookingForm(GeneratorsUtil.GenerateBookingMiddleware(this));
                if (save_result is not null && save_result.Operation_Succeeded && save_result.Result!.Value)
                {
                    Has_Saved = true;
                }// ELSE ERROR TODO
                Is_Saving = false;
                NotifyStateChanged();
            }
        }

        /// <summary>
        /// Reload to get the new data for the Booking
        /// </summary>
        /// <param name="CompleteBookingData">Object of the Booking</param>
        /// <returns></returns>
        public async Task<CompleteBookingData?> ReloadBookingData(int clientID, Guid BookingUUID, EStatusContratacion? status, bool IsForce)
        {
            var requestData = new BookingRequestModel()
            {
                Client_ID = Client_ID,
                Booking_UUID = BookingUUID,
                Language = LanguageUtil.getCurrentCulture(),
                Status = status!.Value,
                IsCimaCimplexEdit = IsForce,
            };

            var formData = await DATA_ACCESS.PostBookingData(requestData);

            if (formData is not null && formData.Operation_Succeeded)
            {
                var bookingInitializer = JsonConvert.DeserializeObject<CompleteBookingData>(formData.Result.Serialized_Data);
                
                return bookingInitializer;
            }

            return new CompleteBookingData();
        }

        /// <summary>
        /// Save Booking Complete
        /// </summary>
        /// <param name="PortCapitalData">Is contract is with port capital send This</param>
        /// <returns></returns>
        public async Task<string?> SaveFinalBookingData(ContractPortCapital? PortCapitalData = null)
        {
            string message = null;
            if(Is_Saving) return message;

            Readonly = true;
            Is_Saving = true;
            ShowModal = true;
            Status = EStatusContratacion.DATA_COMPLETED;
            NotifyStateChanged();
            var save_result = await DATA_ACCESS.PostSaveBookingForm(GeneratorsUtil.GenerateBookingMiddleware(this));
            if (save_result is not null && save_result.Operation_Succeeded && save_result.Result!.Value)
            {
                if(PortCapitalData is not null)
                {
                    await SavePortCapitalPCB(PortCapitalData);
                }

                Has_Saved = true;
                PDF_Ready = true;

                ShowMessage("msg_booking_successfully_saved", Severity.Success);
            }
            else
            {
                Status = EStatusContratacion.SAVED;
                Readonly = false;

                if (Elements.Keys.Any(key => key == 1092 || key == 1164))
                {
                    message = save_result.Errors.First();
                }
            }
            Is_Saving = false;
            ShowModal = false;
            NotifyStateChanged();

            return message;
        }

        //private string GetMessageBookingSaved()
        //{
        //    //if()
        //    return "msg_booking_successfully_saved";
        //}

        private void ShowMessage(string message, Severity severityType = Severity.Error)
        {
            _snackbarServices.Configuration.PositionClass = Defaults.Classes.Position.BottomLeft;
            _snackbarServices.Add(_localizeService.Get(message), severityType);
        }

        public async Task SavePortCapitalPCB(ContractPortCapital Data)
        {
            var ResultContract = await DATA_ACCESS.PostPCB(new RequestPortCapitalBookingForm()
            {
                IsExpress = Data.IsExpress,
                IDCustomer = Data.UserData.IDCustomer,
                IDUser = Data.UserData.IDUser,
                GUID_Contratacion = UUID,
                QuotationSelected = new()
                {
                    Installment_interests_id = Data.QuotationSelected!.Installment_interest_id,
                    Amount = Data.QuotationSelected.Amount,
                    Interest_amount = Data.QuotationSelected.Interest,
                    Vat_interest = Data.QuotationSelected.Vat_amount,
                    Total_amount = Data.QuotationSelected.Total_amount,
                    Currency_key = Data.QuotationSelected.Coin_key,
                },
                Installment = Data.QuotationSelected,
            });

            IsPCBSaving = ResultContract.Operation_Succeeded && ResultContract.Result != null;
            InstallmentSelected = Data.QuotationSelected;
        }

        public bool GetIsPCBSaving() => IsPCBSaving;

        public InstallmentInterests GetInstallmentSelected() =>InstallmentSelected;

        public async Task TryFoundPCB()
        {
            var Result = await DATA_ACCESS.PostHasPCBContracted(UUID);

            if (Result.Operation_Succeeded && Result != null && Result.Result.Succes && Result.Result.InstallmentSelected != null) 
            {
                InstallmentSelected = Result.Result.InstallmentSelected;
            }
        }

        public bool GetBookingIsSaving() => Is_Saving;

        public void SetBookingIsSaving(bool isSaving)
        {
            Is_Saving = isSaving;
            NotifyStateChanged();
        }

        public void UpdateGlobalValidationStatus()
        {
            if (IsForceEdit) ForceUpdateValidGlobalValidationSatus();
            Booking_Data_Complete = Sections.SelectMany(section_groups => section_groups.Value.Values).All(section => section.GetSectionValidated());
            NotifyStateChanged();
        }

        private void ForceUpdateValidGlobalValidationSatus()
        {
            foreach (var sectionGroup in Sections)
            {
                foreach (var section in sectionGroup.Value.Values)
                {
                    if (!section.GetSectionValidated())
                    {
                        section.SetValidationState(true);
                    }
                }
            }
        }

        public bool GetBookingDataComplete() => Booking_Data_Complete;

        public void WriteSharedChip(MudChip? value, int Element_ID)
            => Shared_Chip_Data_Storage[Element_ID] = value;

        public void WriteUnsharedChip(MudChip? value, int Section_ID, Guid Inner_Section_ID, int Element_ID)
            => Unshared_Chip_Data_Storage[Section_ID][Inner_Section_ID][Element_ID] = value;

        public MudChip? GetSharedChip(int Element_ID)
            => Shared_Chip_Data_Storage[Element_ID];

        public MudChip? GetUnsharedChip(int Section_ID, Guid Inner_Section_ID, int Element_ID)
            => Unshared_Chip_Data_Storage[Section_ID][Inner_Section_ID][Element_ID];

        public IDictionary<int, object?> GetSharedStorage() => Shared_Element_Data_Storage;

        public IDictionary<int, IDictionary<int, object?>> GetSharedMetadataStorage() => Shared_Metadata_Storage;

        public IDictionary<int, FullDocumentData> GetSharedDocumentStorage() => Shared_Document_Data_Storage;

        public IDictionary<int, IDictionary<Guid, IDictionary<int, FullDocumentData>>> GetUnsharedDocumentStorage() => Unshared_Document_Data_Storage;
        
        public async Task WriteSharedDocument(FullDocumentData data, int DocumentID)
        {
            Shared_Document_Data_Storage[DocumentID] = data;
            await SaveBookingData();
        }

        public FullDocumentData GetSharedDocument(int DocumentID) => Shared_Document_Data_Storage[DocumentID];

        public async Task WriteUnsharedDocument(FullDocumentData data, int Section_ID, Guid Inner_Section_ID, int DocumentID)
        {
            Unshared_Document_Data_Storage[Section_ID][Inner_Section_ID][DocumentID] = data;
            await SaveBookingData();
        }

        public FullDocumentData GetUnsharedDocument(int Section_ID, Guid Inner_Section_ID, int DocumentID) 
            => Unshared_Document_Data_Storage[Section_ID][Inner_Section_ID][DocumentID];

        public IDictionary<int, IDictionary<Guid, IDictionary<int, bool>>> GetUnsharedComplexValid()
            => Unshared_Complex_Element_Valid;

        public void SetUnsharedComplexValid(bool value, int Section_ID, Guid Section_UUID, int Element_ID)
        {
            if (Unshared_Complex_Element_Valid.TryGetValue(Section_ID, out var _))
            {
                if (Unshared_Complex_Element_Valid[Section_ID].TryGetValue(Section_UUID, out _))
                {
                    Unshared_Complex_Element_Valid[Section_ID][Section_UUID][Element_ID] = value;
                }
                else
                {
                    Unshared_Complex_Element_Valid[Section_ID][Section_UUID] = new Dictionary<int, bool>();
                    Unshared_Complex_Element_Valid[Section_ID][Section_UUID][Element_ID] = value;
                }
            }
            else
            {
                Unshared_Complex_Element_Valid[Section_ID] = new Dictionary<Guid, IDictionary<int, bool>>();
                Unshared_Complex_Element_Valid[Section_ID][Section_UUID] = new Dictionary<int, bool>();
                Unshared_Complex_Element_Valid[Section_ID][Section_UUID][Element_ID] = value;
            }
        }

        public IDictionary<int, bool> GetSharedComplexValid() => Shared_Complex_Element_Valid;

        public void SetSharedComplexValid(bool value, int Element_ID)
            => Shared_Complex_Element_Valid[Element_ID] = value;

        public IDictionary<int, IDictionary<Guid, IDictionary<int, object?>>> GetUnsharedStorage()
            => Unshared_Element_Data_Storage;

        public IDictionary<int, IDictionary<Guid, IDictionary<int, IDictionary<int, object?>>>> GetUnsharedMetadataStorage()
            => Unshared_Metadata_Storage;

        public bool GetSectionGroupComplete(int SectionID)
            => Sections[SectionID].Values.All(inner_section => inner_section.GetSectionValidated());

        public bool GetIsInitialized() => Initialized;

        public bool IsServiceNoGenerateBookink() => ServiceSettings.ServiceNoGenerateBookink;
    }
}
