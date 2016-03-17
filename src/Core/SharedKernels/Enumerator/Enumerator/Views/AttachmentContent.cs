namespace WB.Core.SharedKernels.Enumerator.Views
{
    public class AttachmentContent 
    {
        public string Id { get; set; }

        public string ContentType { get; set; }

        public long Size { get; set; }

        public byte[] Content { get; set; }
    }
}