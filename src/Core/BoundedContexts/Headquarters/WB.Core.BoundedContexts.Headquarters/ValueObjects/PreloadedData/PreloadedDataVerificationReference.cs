namespace WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData
{
    public class PreloadedDataVerificationReference
    {
        public PreloadedDataVerificationReference(long? positionX, long? positionY, PreloadedDataVerificationReferenceType type, string content, string dataFile)
        {
            this.DataFile = dataFile;
            this.PositionX = positionX;
            this.PositionY = positionY;
            this.Type = type;
            this.Content = content;
        }

        public PreloadedDataVerificationReference(PreloadedDataVerificationReferenceType type, string content, string dataFile)
        {
            this.DataFile = dataFile;
            this.Type = type;
            this.Content = content;
        }

        public PreloadedDataVerificationReferenceType Type { get; private set; }
        public long? PositionX { get; private set; }
        public long? PositionY { get; private set; }
        public string Content { get; private set; }
        public string DataFile { get; private set; }
    }
}
