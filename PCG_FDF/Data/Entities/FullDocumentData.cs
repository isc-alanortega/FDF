using Microsoft.AspNetCore.Components.Forms;
using PCG_ENTITIES.Enums;
using PCG_ENTITIES.PCG_FDF.UtilityEntities;

namespace PCG_FDF.Data.Entities
{
    public class FullDocumentData : IDocumentData
    {
        /// <summary>
        /// Indica si el elemento ya se encuentra subido
        /// </summary>
        public bool Document_Uploaded { get; set; }
        /// <summary>
        /// Contiene el archivo a subir o un placeholder
        /// </summary>
        public IBrowserFile? Document_File { get; set; }
        /// <summary>
        /// ID del documento que se registró al guardarse
        /// </summary>
        public int? Uploaded_Document_ID { get; set; }
        /// <summary>
        /// Razón por la que se rechazó el documento
        /// </summary>
        public EDocumentRejections? Document_Rejection_Reason { get; set; }
        /// <summary>
        /// Estado del documento
        /// </summary>
        public EDocumentStatus? Document_Status { get; set; }
    }
}
