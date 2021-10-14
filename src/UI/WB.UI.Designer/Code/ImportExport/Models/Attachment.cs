using System;

namespace WB.UI.Designer.Code.ImportExport.Models
{
    public class Attachment
    {
        public Guid AttachmentId { get; set; }
        public string Name { get; set; } = String.Empty;
        public string ContentId { get; set; } = String.Empty;

        public Attachment Clone()
        {
            return new Attachment
            {
                AttachmentId = this.AttachmentId,
                Name = this.Name,
                ContentId = this.ContentId
            };
        }
    }
}
