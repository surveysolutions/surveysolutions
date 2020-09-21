using System.IO;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class EmailAttachment
    {
        public string Filename { get; set; }
        public string Base64String { get; set; }
        public string ContentType { get; set; }
    }
}