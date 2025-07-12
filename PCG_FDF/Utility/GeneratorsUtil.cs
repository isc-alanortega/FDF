using MudBlazor;
using PCG_ENTITIES.PCG;
using PCG_ENTITIES.PCG_FDF.BookingEntities;
using PCG_ENTITIES.PCG_FDF.ContractSettigs;
using PCG_ENTITIES.PCG_FDF.QuotationEntities;
using PCG_ENTITIES.PCG_FDF.UtilityEntities;
using PCG_FDF.Data.ComponentDI;
using PCG_FDF.Data.ComponentDI.Booking;
using PCG_FDF.Data.ComponentDI.Quotation;
using PCG_FDF.Data.Entities;

namespace PCG_FDF.Utility
{
    public static class GeneratorsUtil
    {
        public static List<BreadcrumbItem> GenerateBreadcrumb(IList<ComplexService> services)
            => services.Select(service => new BreadcrumbItem(
                text: service.Name, 
                href: null, 
                disabled: true
            )).ToList();
                
        public static IList<ComplexService> GenerateComplexServiceFromTrace(ServiciosTraceEntidad trace)
        {
            var services = trace.ServiciosTrace.Select(categoria => new ComplexService(
                id: categoria.ID,
                code: categoria.Codigo,
                version: 0, 
                name: categoria.Nombre_corto,
                fullName: categoria.Nombre,
                alias: null,
                calculo_cantidad: null,
                icon: categoria.Icon,
                viewbox: categoria.Viewbox,
                level: -100,
                isActionable: true,
                helpTooltip: categoria.Descripcion,
                _PortCapital: false,
                parent: null,
                children: new Dictionary<Guid, ComplexService>(),
                subservicios: new List<SubserviciosEntidad>(),
                tipoClientePC: "N"
            )).ToList();

            services.Add(new ComplexService(
                id: trace.Servicio.Servicio.ID,
                code: trace.Servicio.Servicio.Codigo,
                version: trace.Servicio.Servicio.Version,
                name: trace.Servicio.Servicio.Nombre_corto,
                fullName: trace.Servicio.Servicio.Nombre,
                alias: trace.Servicio.Servicio.Alias,
                calculo_cantidad: trace.Servicio.Servicio.CalculoCantidad,
                icon: trace.Servicio.Servicio.Icon,
                viewbox: trace.Servicio.Servicio.Viewbox,
                level: -100,
                isActionable: true,
                helpTooltip: trace.Servicio.Servicio.Descripcion,
                _PortCapital: trace.Servicio.Servicio.PortCapital, 
                parent: null,
                children: new Dictionary<Guid, ComplexService>(),
                subservicios: trace.Servicio.Subservicios, 
                tipoClientePC: trace.Servicio.Servicio.TipoClientePC, 
                noDescargarBooking: trace.Servicio.Servicio.NoDescargarBooking,
                noShowQuotation: trace.Servicio.Servicio.NoMostrarCotizacion,
                requireBillOfLading: trace.Servicio.Servicio.RequireBillOfLanding
            ));

            return services;
        }

        public static BookingMiddleware GenerateBookingMiddleware(BookingDataCollection bookingData) => new()
        {
            GUID_Contratacion = bookingData.UUID,
            User_ID = bookingData.User_ID,
            Booking_Page = bookingData.GetBookingPage(),
            Booking_URI = bookingData.GetBookingURI(),
            Client_ID = bookingData.Client_ID,
            Folio = bookingData.Invoice,
            Booking_Data_Language = bookingData.GetBookingLanguage(),
            Sections = bookingData.GetFormSections().ToDictionary(key => key.Key, value => (IDictionary<Guid, StatefulSectionMiddleware>)value.Value.ToDictionary(key => key.Key, value => GenerateSectionMiddleware(value.Value))),
            Elements = bookingData.GetElements(),
            Documents = bookingData.GetDocuments(),
            Section_Relationship_Elements = bookingData.GetElementRelationships(),
            Section_Relationship_Documents = bookingData.GetDocumentRelationships(),
            Unshared_Complex_Element_Valid = bookingData.GetUnsharedComplexValid(),
            Shared_Complex_Element_Valid = bookingData.GetSharedComplexValid(),
            Shared_Element_Data_Storage = bookingData.GetSharedStorage(),
            Shared_Document_Data_Storage = bookingData.GetSharedDocumentStorage().ToDictionary(key => key.Key, value => new DocumentData()
            {
                Document_Uploaded = value.Value.Document_Uploaded,
                Uploaded_Document_ID = value.Value.Uploaded_Document_ID,
                Document_Rejection_Reason = value.Value.Document_Rejection_Reason,
                Document_Status = value.Value.Document_Status,
            }),
            Unshared_Document_Data_Storage = bookingData.GetUnsharedDocumentStorage().ToDictionary(key => key.Key, value =>
            (IDictionary<Guid, IDictionary<int, DocumentData>>)value.Value.ToDictionary(key => key.Key, value =>
            (IDictionary<int, DocumentData>)value.Value.ToDictionary(key => key.Key, value => new DocumentData()
            {
                Document_Uploaded = value.Value.Document_Uploaded,
                Uploaded_Document_ID = value.Value.Uploaded_Document_ID,
                Document_Rejection_Reason = value.Value.Document_Rejection_Reason,
                Document_Status = value.Value.Document_Status
            }))),
            Shared_Metadata_Storage = bookingData.GetSharedMetadataStorage(),
            Unshared_Element_Data_Storage = bookingData.GetUnsharedStorage(),
            Unshared_Metadata_Storage = bookingData.GetUnsharedMetadataStorage(),
            Services = bookingData.GetServices(),
            Readonly = bookingData.GetIsReadonly(),
            Booking_Data_Complete = bookingData.GetBookingDataComplete(),
            ServicesSettigs = bookingData.ServiceSettings,
            IsEdit = bookingData.IsForceEdit,
        };

        private static StatefulSectionMiddleware GenerateSectionMiddleware(StatefulSection sectionData) => new()
        {
            Section_ID = sectionData.Section_ID,
            From_Service = sectionData.From_Service,
            Section_GUID = sectionData.Section_GUID,
            Has_Chips = sectionData.Has_Chips,
            Section_Name = sectionData.Section_Name,
            Section_Icon = sectionData.Section_Icon,
            Icon_ViewBox = sectionData.Icon_ViewBox,
            Section_Sequence = sectionData.Section_Sequence,
            Multiply_By_Quantity = sectionData.Multiply_By_Quantity,
            Data_Complete = sectionData.GetSectionValidated()
        };

        public static QuotationMiddleware GenerateQuotationMiddleware(QuotationDataCollection clientData, bool is_final = false) =>new()
        {
            Endpoint = clientData.Endpoint,
            UUID = clientData.UUID,
            Invoice = clientData.Invoice,
            User_ID = clientData.User_ID,
            Location_ID = clientData.Location_ID,
            Client_ID = clientData.Client_ID,
            Money_Kind = clientData.Money_Kind,
            Has_Saved = clientData.GetQuotationHasSaved(),
            Quotation_URI = clientData.GetQuotationURI(),
            All_Valid_Tariffs = clientData.GetAllValidTariffs(),
            Definitive = is_final,
            Totals = clientData.GetTotals(),
            Shared_Element_Data_Storage = clientData.GetStorage(),
            Services_Rel_Cards = clientData.GetServicesCards(),
            Packages_Rel_Cards = clientData.GetPackagesCards(),
            Subservices_Data = clientData.GetSubservicesData().ToDictionary(key => key.Key, value => (IDictionary<int, StatefulSubserviceMiddleware>)value.Value.ToDictionary(key => key.Key, value => GenerateSubservice(value.Value))),
            Cards = clientData.GetCards().ToDictionary(key => key.Key, value => GenerateCard(value.Value)),
            Service_Data_Ready = clientData.GetServiceDataReady(),
            Service_Tariff_Fields = clientData.GetServiceTariffFields(),
            Quotation_Services = clientData.GetQuotationServices(),
            Quotation_Packages_Services = clientData.GetQuotationPackageServices(),
            Quotation_Packages_Services_Reverse = clientData.GetPackagesReverseLookup(),
            Services_Tariffs = clientData.GetServicesTariffs(),
            Packages_Tariffs = clientData.GetPackagesTariffs(),
            ServiceWithoutQuotation = clientData.IsServiceWithoutQuotation(),
            ServicesSettings = new QuotationServicesSettigs()
            {
                ServiceWithoutQuotation = clientData.IsServiceWithoutQuotation(),
                ServiceRequireBillOfLanding = clientData.IsServiceRequireBillOfLanding(),
                ServiceIsPortCapital = clientData.GetIsPortCapital(),
                ServiceNoGenerateBookink = clientData.IsServiceNoGenerateBookink(),
            }
        };

        private static StatefulSubserviceMiddleware GenerateSubservice(StatefulSubservice data) => new()
        {
            Subservice_ID = data.Subservice_ID,
            Selected = data.Selected,
            Effects_Tariff = data.Effects_Tariff,
            Icon = data.Icon,
            Name = data.Name,
            Version = data.Version,
            ViewBox = data.ViewBox
        };

        private static StatefulCardMiddleware GenerateCard(StatefulCard data) => new()
        {
            Card_Icon = data.Card_Icon,
            Card_Name_ES = data.Card_Name_ES,
            Card_Icon_ID = data.Card_Icon_ID,
            Card_Sequence = data.Card_Sequence,
            Card_ID = data.Card_ID,
            Card_Name_EN = data.Card_Name_EN,
            Card_Name_ID = data.Card_Name_ID,
            Data_Complete = data.GetCardValidated(),
            Elements = data.Elements,
            From_Services = data.From_Services,
            Icon_ViewBox = data.Icon_ViewBox,
            Tariff_Fields = data.Tariff_Fields,
            Chips_Complete = data.GetChipsValidation()
        };

        public static MiddlewareQuotation GenerateQuotationObject(QuotationDataCollection_LEGACY clientData, bool is_update) => new()
        {
            Pay_CIMA_Coins = clientData.GetUseCIMACoins(),
            is_update = is_update,
            Quotation_URI = clientData.GetQuotationURI(),
            Quotation_Page = clientData.GetQuotationPage(),
            money_kind = clientData.GetQuotationMoneyKind(),
            language = clientData.GetQuotationLanguage(),
            Quotation_GUID = clientData.GetQuotationGUID(),
            Creation_Date = clientData.GetQuotationCreationDate(),
            service_data_ready = clientData.GetServiceDataReady(),
            User_ID = clientData.GetQuotationUserID(),
            Client_ID = clientData.GetQuotationClientID(),
            Coins_Rate = clientData.GetQuotationCoinsRate(),
            Totals = clientData.GetTotals(),
            serviceElement_pairs = clientData.GetServiceElementPairs(),
            quotation_data = clientData.GetQuotationData().ToDictionary(kvp => kvp.Key, kvp => ConvertToReadOnly(kvp.Value)),
            subservices_data = clientData.GetSubservicesData(),
            campos_tarifarios = clientData.GetTarifaFields(),
            tarifas_servicios = clientData.GetServiceTariffs(),
            tarifas_paquete = clientData.GetPackagesTariffs(),
            service_ids = clientData.GetServiceIds(),
            package_ids = clientData.GetPackageIds(),
            package_services_ids = clientData.GetPackageServicesIds()
        };

        private static MiddlewareElement ConvertToReadOnly(ElementQuotationData data)
        {
            var element = new MiddlewareElement()
            {
                ELEMENT_ID = data.GetElementID(),
                from_services = data.GetAssociations(),
                exclusions = data.GetElementExclusions(),
                _all_answered = data.AllFieldsAnswered(),
                field_types = data.GetFieldTypes(),
            };

            if (data.GetIntFields() != null && data.GetIntFields()!.Any())
            {
                element.int_data = data.GetIntFields();
            }
            if (data.GetDoubleFields() != null && data.GetDoubleFields()!.Any())
            {
                element.double_data = data.GetDoubleFields();
            }
            if (data.GetDateTimeFields() != null && data.GetDateTimeFields()!.Any())
            {
                element.dateTime_data = data.GetDateTimeFields();
            }
            if (data.GetStringFields() != null && data.GetStringFields()!.Any())
            {
                element.string_data = data.GetStringFields();
            }
            if (data.GetBoolFields() != null && data.GetBoolFields()!.Any())
            {
                element.bool_data = data.GetBoolFields();
            }
            if (data.GetGuidFields() != null && data.GetGuidFields()!.Any())
            {
                element.guid_data = data.GetGuidFields();
            }
            if (data.GetComplex() != null && data.GetComplex()!.Any())
            {
                element.complex_data = data.GetComplex();
            }

            return element;
        }
    }
}
