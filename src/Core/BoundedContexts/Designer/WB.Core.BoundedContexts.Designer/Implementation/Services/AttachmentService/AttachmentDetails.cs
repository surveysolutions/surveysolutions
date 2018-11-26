using System.Drawing.Imaging;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService
{
    public class AttachmentDetails
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public byte[] Thumbnail { get; set; }
    }
}
