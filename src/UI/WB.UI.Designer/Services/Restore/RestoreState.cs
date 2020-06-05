using System;
using System.Collections.Generic;
using System.Text;
using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Designer.Services.Restore
{
    public class RestoreState
    {
        public class Attachment
        {
            public string? FileName { get; set; }
            public string? ContentType { get; set; }
            public byte[]? BinaryContent { get; set; }

            public bool HasAllDataForRestore()
                => this.FileName != null
                   && this.ContentType != null
                   && this.BinaryContent != null;
        }

        private readonly Dictionary<Guid, Attachment> attachments = new Dictionary<Guid, Attachment>();

        public int RestoredEntitiesCount { get; set; }

        public void StoreAttachmentContentType(Guid attachmentId, string contentType)
        {
            this.attachments.GetOrAdd(attachmentId).ContentType = contentType;
        }

        public void StoreAttachmentFile(Guid attachmentId, string fileName, byte[] binaryContent)
        {
            this.attachments.GetOrAdd(attachmentId).FileName = fileName;
            this.attachments.GetOrAdd(attachmentId).BinaryContent = binaryContent;
        }

        public Attachment GetAttachment(Guid attachmentId)
            => this.attachments[attachmentId];

        public void RemoveAttachment(Guid attachmentId)
            => this.attachments.Remove(attachmentId);

        public IEnumerable<Guid> GetPendingAttachments()
            => this.attachments.Keys;

        public StringBuilder Success { get; } = new StringBuilder();
        public string? Error { get; set; }
    }
}
