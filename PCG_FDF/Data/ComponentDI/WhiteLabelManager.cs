using PCG_ENTITIES.Enums;
using PCG_ENTITIES.PCG_FDF.UtilityEntities;
using PCG_FDF.Data.DataAccess;
using PCG_FDF.Data.Entities;
using PCG_FDF.Data.Localization;

namespace PCG_FDF.Data.ComponentDI
{
    public class WhiteLabelManager
    {
        public EPages Current_Page { get; private set; }
        public ThemeData Current_Theme_Data { get; private set; }
        public IDictionary<string, bool> Config { get; private set; }
        public bool Initialized { get; private set; }
        private readonly BaseUriDI Base_Uri_Access;
        private readonly PCG_FDF_DB DATA_ACCESS;
        private readonly GlobalLocalizer localize;

        public WhiteLabelManager(PCG_FDF_DB data_access, BaseUriDI base_access, GlobalLocalizer _localize)
        {
            Base_Uri_Access = base_access;
            DATA_ACCESS = data_access;
            localize = _localize;

            switch (Base_Uri_Access.Current_Uri)
            {
                case "cimashipping.centralinformatica.com":
                    {
                        Current_Page = EPages.Cima_Shipping;
                        break;
                    }
                case "bafar.cimagroup.online":
                    {
                        Current_Page = EPages.Grupo_Bafar;
                        break;
                    }
                case "thesharks.cimagroup.online":
                    {
                        Current_Page = EPages.The_Sharks;
                        break;
                    }
                case "evergreen.cimagroup.online":
                    {
                        Current_Page = EPages.Evergreen;
                        break;
                    }
                case "smargo.cimagroup.online":
                    {
                        Current_Page = EPages.SmarGo;
                        break;
                    }
                case "servicios.cargopi.com":
                    {
                        Current_Page = EPages.CargoPi;
                        break;
                    }
                default:
                    Current_Page = EPages.Cima_Group;
                    break;
            }
        }

        public async Task Initialize()
        {
            if (!Initialized)
            {
                Current_Theme_Data = await DATA_ACCESS.GetThemeData(Base_Uri_Access.Current_Uri);
                Config = await DATA_ACCESS.GetSiteConfig(Base_Uri_Access.Current_Uri);
                Initialized = true;
            }
        }

        public bool GetUseCimaCoins()
        {
            if (Config is not null && Config.Any())
            {
                return Config.TryGetValue("Usar Coins", out bool result) ? result : false;
            }
            return false;
        }

        public bool GetUsePortCapital()
        {
            bool banPortCapital = false;
            if (Config is not null && Config.Any())
            {
                banPortCapital = Config.TryGetValue("Usar Port Capital", out bool result) ? result : false;
            }
            return banPortCapital;
        }

        public bool GetUseChat()
        {
            if (Config is not null && Config.Any())
            {
                return Config.TryGetValue("Enable Chat", out bool result) ? result : false;
            }
            return false;
        }

        public CustomIcon GetPageIcon()
        {
            switch (Current_Page)
            {
                case EPages.Cima_Shipping:
                    return CustomIcons.CIMA_Shipping_Colored;
                case EPages.Grupo_Bafar:
                    return CustomIcons.Bafar_Uncolored;
                case EPages.Cima_Group:
                    return CustomIcons.cimaLogoSimpleColored;
                case EPages.The_Sharks:
                    return CustomIcons.Sharks_Horizontal;
                case EPages.Evergreen:
                    return CustomIcons.Evergreen_Pin;
                case EPages.SmarGo:
                    return CustomIcons.SmarGo_Uncolored;
                case EPages.CargoPi:
                    return CustomIcons.CargoPi_Light_Full;
                default:
                    return CustomIcons.cimaLogoSimpleColored;
            }
        }

        public CustomIcon GetBookingIcon()
        {
            switch (Current_Page)
            {
                default:
                    return CustomIcons.cimaBookingLogo;
            }
        }

        public CustomIcon GetLandingQuoteIcon()
        {
            switch (Current_Page)
            {
                default:
                    return CustomIcons.landingQuoteIcon;
            }
        }

        public CustomIcon GetLandingContractIcon()
        {
            switch (Current_Page)
            {
                default:
                    return CustomIcons.landingContractIcon;
            }
        }

        public CustomIcon GetLandingTrackingIcon()
        {
            switch (Current_Page)
            {
                default:
                    return CustomIcons.landingTrackingIcon;
            }
        }

        public CustomIcon GetFooterIcon()
        {
            switch (Current_Page)
            {
                case EPages.Cima_Shipping:
                    return CustomIcons.CIMA_Shipping_Colored;
                case EPages.Grupo_Bafar:
                    return CustomIcons.Bafar_Uncolored;
                case EPages.Cima_Group:
                    return CustomIcons.cimaLogoSimpleColored;
                case EPages.The_Sharks:
                    return CustomIcons.Sharks_Horizontal;
                case EPages.Evergreen:
                    return CustomIcons.Evergreen_Pin;
                case EPages.SmarGo:
                    return CustomIcons.SmarGo_Uncolored;
                case EPages.CargoPi:
                    return CustomIcons.CargoPi_Light_Simple;
                default:
                    return CustomIcons.cimaLogoSimpleColored;
            }
        }

        public string GetTextColorFromBackgroundColor(string background_color)
        {
            var hexColor = Current_Theme_Data.MudBlazor_Theme_Data[background_color];

            var r = Convert.ToInt32(hexColor.Substring(1, 2), 16);
            var g = Convert.ToInt32(hexColor.Substring(3, 2), 16);
            var b = Convert.ToInt32(hexColor.Substring(5, 2), 16);

            var yiq = ((r * 299) + (g * 587) + (b * 114)) / 1000;

            return yiq >= 128 ? "t-black" : "t-white";
        }


        public string GetContactPhone()
        {
            switch (Current_Page)
            {
                case EPages.Cima_Shipping:
                    return "+52(314) 3367 264";
                case EPages.Grupo_Bafar:
                    return "(01800) 627-7705";
                case EPages.Cima_Group:
                    return "+52(314) 3367 264";
                case EPages.The_Sharks:
                    return "+52(314) 157 1962";
                case EPages.Evergreen:
                    return "+52(314) 331 3382";
                case EPages.SmarGo:
                    return "+52(314) 659 3547";
                default:
                    return "+52(314) 3367 264";
            }
        }

        public string GetContactEmail()
        {
            switch (Current_Page)
            {
                case EPages.Cima_Shipping:
                    return "contacto@cimabooking.com.mx";
                case EPages.Grupo_Bafar:
                    return "atencionclientes@bafar.com.mx";
                case EPages.Cima_Group:
                    return "contacto@cimagroup.com.mx";
                case EPages.The_Sharks:
                    return "gisell.montiel@thesharks.com.mx";
                case EPages.Evergreen:
                    return "mzo@evergreen-shipping.com.mx";
                case EPages.SmarGo:
                    return "contacto@smargo.com.mx";
                default:
                    return "contacto@cimagroup.com.mx";
            }
        }

        public string GetContactLocation()
        {
            switch (Current_Page)
            {
                case EPages.Cima_Shipping:
                    return "Carretera federal libre Manzanillo - Minatitlán Km. 4.5 Colonia, Tapeixtles C.P. 28239 Manzanillo, Colima, México";
                case EPages.Grupo_Bafar:
                    return "Km. 7.5 Carretera a Cuauhtémoc en la Colonia Las Ánimas, C.P. 31450 en Chihuahua, Chihuahua";
                case EPages.Cima_Group:
                    return "Carretera federal libre Manzanillo - Minatitlán Km. 4.5 Colonia, Tapeixtles C.P. 28239 Manzanillo, Colima, México";
                case EPages.The_Sharks:
                    return "Blvd. Miguel de la Madrid 1185, C.P 28869 Manzanillo, Colima, México";
                case EPages.Evergreen:
                    return "EVERGREEN SHIPPING AGENCY MANZANILLO OFFICE - Constitución No. 7, Piso 1, Burócrata, Manzanillo, Colima, México";
                case EPages.SmarGo:
                    return "Carretera federal libre Manzanillo - Minatitlán Km. 4.5 Colonia, Tapeixtles C.P. 28239 Manzanillo, Colima, México";
                default:
                    return "Carretera federal libre Manzanillo - Minatitlán Km. 4.5 Colonia, Tapeixtles C.P. 28239 Manzanillo, Colima, México";
            }
        }

        public string GetAllRights()
        {
            switch (Current_Page)
            {
                case EPages.Cima_Shipping:
                    return localize.Get("footer_allrights_CS");
                case EPages.Grupo_Bafar:
                    return localize.Get("footer_allrights_GB");
                case EPages.Cima_Group:
                    return localize.Get("footer_allrights_CG");
                case EPages.The_Sharks:
                    return localize.Get("footer_allrights_TS");
                case EPages.Evergreen:
                    return localize.Get("footer_allrights_EG");
                case EPages.SmarGo:
                    return localize.Get("footer_allrights_SG");
                case EPages.CargoPi:
                    return localize.Get("footer_allrights_CP");
                default:
                    return localize.Get("footer_allrights_CG");
            }
        }

        public string GetIconSizeModifier()
        {
            switch (Current_Page)
            {
                case EPages.Cima_Shipping:
                    return "icon-rem-s3";
                case EPages.Grupo_Bafar:
                    return "icon-rem-s3";
                case EPages.Cima_Group:
                    return "";
                case EPages.The_Sharks:
                    return "icon-rem-s14";
                case EPages.Evergreen:
                    return "icon-rem-s3_7";
                case EPages.SmarGo:
                    return "icon-rem-s13";
                case EPages.CargoPi:
                    return "icon-rem-s15";
                default:
                    return "";
            }
        }

        public string GetIconFooterSizeModifier()
        {
            switch (Current_Page)
            {
                case EPages.Cima_Shipping:
                    return "icon-rem-s3";
                case EPages.Grupo_Bafar:
                    return "icon-rem-s3";
                case EPages.Cima_Group:
                    return "icon-rem-s3";
                case EPages.The_Sharks:
                    return "icon-rem-s14";
                case EPages.Evergreen:
                    return "icon-rem-s4";
                case EPages.SmarGo:
                    return "icon-rem-s15";
                case EPages.CargoPi:
                    return "icon-rem-s6";
                default:
                    return "icon-rem-s3";
            }
        }

        public string GetIconAgreementSizeModifier()
        {
            switch (Current_Page)
            {
                case EPages.Cima_Shipping:
                    return "icon-rem-s5";
                case EPages.Grupo_Bafar:
                    return "icon-rem-s5";
                case EPages.Cima_Group:
                    return "";
                case EPages.The_Sharks:
                    return "icon-rem-s14";
                case EPages.Evergreen:
                    return "icon-rem-s4";
                case EPages.SmarGo:
                    return "icon-rem-s13";
                default:
                    return "";
            }
        }

        public string GetIconCSSClass()
        {
            switch (Current_Page)
            {
                case EPages.Cima_Shipping:
                    return "internal-shipping-icon";
                case EPages.Grupo_Bafar:
                    return "internal-bafar-icon";
                case EPages.Cima_Group:
                    return "internal-cima-icon";
                case EPages.The_Sharks:
                    return "internal-thesharks-icon remove-height-em";
                case EPages.Evergreen:
                    return "internal-evergreen-icon";
                case EPages.SmarGo:
                    return "internal-smargo-icon remove-height-em";
                default:
                    return "internal-cima-icon";
            }
        }

        public string GetPageName()
        {
            switch (Current_Page)
            {
                case EPages.Cima_Shipping:
                    return "CIMA SHIPPING";
                case EPages.Grupo_Bafar:
                    return "GRUPO BAFAR";
                case EPages.Cima_Group:
                    return "CIMA Booking";
                case EPages.The_Sharks:
                    return "THE SHARKS";
                case EPages.Evergreen:
                    return "EVERGREEN";
                case EPages.SmarGo:
                    return "SmarGo";
                case EPages.CargoPi:
                    return "Cargo Pi";
                default:
                    return "CIMA Booking";
            }
        }

        public string GetPageURI()
        {
            return Base_Uri_Access.Current_Uri;
        }

        public int? GetPageID()
        {
            if (Current_Page == EPages.Cima_Group)
            {
                return null;
            }
            else
            {
                return (int)Current_Page;
            }
        }
    }
}
