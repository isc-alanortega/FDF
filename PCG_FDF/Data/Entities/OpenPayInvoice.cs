using PCG_ENTITIES.PCG;

namespace PCG_FDF.Data.Entities
{
    public class OpenPayInvoices
    {
        public string Amount { get; set; }
        public IEnumerable<OpenPayInvoiceData> Invoices { get; set; }
    }

    public class OpenPayInvoiceData
    {
        public int Header_Invoice_ID { get; set; }
        public string Invoice { get; set; }
        public string Company_Name { get; set; }
        public string Date { get; set; }
        public decimal Payment_Amount { get; set; }
    }
}
