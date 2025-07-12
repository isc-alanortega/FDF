using PCG_ENTITIES.Enums;
using System.Globalization;

namespace PCG_FDF.Utility
{
    public static class LanguageUtil
    {
        private static IDictionary<string, string> cultures = new Dictionary<string, string>() {
            { "es-MX", "ES" },
            { "en-US", "EN" }
        };

        public static ELanguage Language { get; set; }

        public static string getCurrentCulture()
        {
            return cultures[CultureInfo.CurrentCulture.Name];
        }

        public static void SetCurrentLanguage()
        {
            if (cultures[CultureInfo.CurrentCulture.Name] == "ES")
            {
                Language = ELanguage.SPANISH;
            }
            else
            {
                Language = ELanguage.ENGLISH;
            }
        }
    }
}
