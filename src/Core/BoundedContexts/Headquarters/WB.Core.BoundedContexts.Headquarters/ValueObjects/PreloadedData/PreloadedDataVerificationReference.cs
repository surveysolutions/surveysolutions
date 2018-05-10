namespace WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData
{
    public class InterviewImportReference
    {
        public InterviewImportReference(string column, long? row, PreloadedDataVerificationReferenceType type, string content, string dataFile)
        {
            this.DataFile = dataFile;
            this.Column = column;
            this.Row = row;
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
        public string Column { get; private set; }
        public long? Row { get; private set; }
        public string Content { get; private set; }
        public string DataFile { get; private set; }
    }
}
