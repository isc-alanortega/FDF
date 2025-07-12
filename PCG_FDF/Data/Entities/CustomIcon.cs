namespace PCG_FDF.Data.Entities
{
    public class CustomIcon
    {
        private readonly string Icon;
        private readonly string ViewBox;

        public CustomIcon(string icon, string viewbox) {
            Icon = icon;
            ViewBox = viewbox;
        }

        public string GetIcon() {
            return Icon;
        }

        public string GetViewBox() {
            return ViewBox;
        }
    }
}
