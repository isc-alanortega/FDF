using Microsoft.AspNetCore.Components.Forms;
using PCG_ENTITIES.PCG_FDF.UtilityEntities;

namespace PCG_FDF.Data.Entities
{
    public class UploadDocumentFormData : IDocumentFormData
    {
        public string Name { get; set; }
        public DateTimeOffset LastModified { get; set; }
        public long Size { get; set; }
        public string ContentType { get; set; }
        public IBrowserFile File { get; set; }
        public int Saved_Document_ID { get; set; }
    }
}
