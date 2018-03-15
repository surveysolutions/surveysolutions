namespace WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData
{
    public class InterviewImportReference
    {
        public InterviewImportReference(long? positionX, long? positionY, PreloadedDataVerificationReferenceType type, string content, string dataFile)
        {
            this.DataFile = dataFile;
            this.PositionX = positionX;
            this.PositionY = positionY;
            this.Type = type;
            this.Content = content;
        }

        public InterviewImportReference(PreloadedDataVerificationReferenceType type, string content, string dataFile)
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
