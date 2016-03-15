namespace WB.Core.GenericSubdomains.Portable.Implementation
{
    public class RestFile
    {
        public RestFile(byte[] content, string contentType, string contentHash, long? contentLength, string fileName)
        {
            this.Content = content;
            this.ContentType = contentType;
            this.ContentHash = contentHash;
            this.ContentLength = contentLength;
            this.FileName = fileName;
        }

        private string FileName { get; }
        public string ContentHash { get; }
        public long? ContentLength { get; }
        public string ContentType { get; }
        public byte[] Content { get; }
    }
}