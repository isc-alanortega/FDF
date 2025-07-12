using Microsoft.AspNetCore.Components.Forms;

namespace PCG_FDF.Data.Entities
{
    public class PlaceholderFile : IBrowserFile
    {
        public readonly int PLACEHOLDER = 0;
        public string Name => "Placeholder.pdf";

        public DateTimeOffset LastModified => DateTimeOffset.UtcNow;

        public long Size => 0;

        public string ContentType => "application/pdf";

        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
        {
            // Return an empty stream
            return new MemoryStream();
        }
    }
}
