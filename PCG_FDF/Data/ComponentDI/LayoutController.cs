namespace PCG_FDF.Data.ComponentDI
{
    public class LayoutController
    {
        public bool Hide_Layout { get; set; } = true;

        public void SetControl(bool value)
        {
            Hide_Layout = value;
        }
    }
}
