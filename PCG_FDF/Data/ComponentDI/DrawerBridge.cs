namespace PCG_FDF.Data.ComponentDI
{
    public class DrawerBridge
    {
        private bool _nav_drawer_binding = false;
        private bool _quotation_drawer_binding = false;
        public event Action CloseQuotation;

        private void ACloseQuotation() => CloseQuotation?.Invoke();

        public void SetNavBind(bool value)
        {
            _nav_drawer_binding = value;
            CheckQuotationBinding();
        }

        private void CheckQuotationBinding() {
            if (_nav_drawer_binding && _quotation_drawer_binding) {
                ACloseQuotation();
            }
        }

        public void SetQuotationBind(bool value)
        {
            _quotation_drawer_binding = value;
        }
    }
}
