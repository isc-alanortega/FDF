using MudBlazor;

namespace PCG_FDF.Utility
{
    public static class ResponsiveUtil
    {
        public static ValueTuple<bool, bool, bool, bool> BreakpointCheck(Breakpoint breakpoint)
        {
            var isMd = breakpoint == Breakpoint.Md;
            var isSm = breakpoint == Breakpoint.Sm;
            var isXs = breakpoint == Breakpoint.Xs;
            var isMobile = isMd || isSm || isXs;
            return (isMobile, isMd, isSm, isXs);
        }

        public static Typo GetMobileVersionOf(Typo normal_res_typo, bool isMobile)
        {
            if (normal_res_typo == Typo.h1 || !isMobile)
            {
                return normal_res_typo;
            }

            return normal_res_typo.Previous();
        }

        public static string GetMobileClass(string baseClass, bool isMobile)
        {
            if (isMobile)
            {
                return $"{baseClass} resize-it";
            }

            return baseClass;
        }
    }
}
