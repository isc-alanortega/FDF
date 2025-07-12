using Microsoft.Extensions.Localization;
using PCG_FDF.Properties;

namespace PCG_FDF.Data.Localization
{
    public class GlobalLocalizer
    {
        private readonly IStringLocalizer _localizer;
        public GlobalLocalizer(IStringLocalizerFactory factory)
        {
            _localizer = factory.Create(typeof(Resources));
        }

        public string Get(string key)
        {
            return _localizer[key];
        }

        public bool TryGet(string key)
        {
            var localized = _localizer[key];
            return !localized.ResourceNotFound;
        }
    }
}
