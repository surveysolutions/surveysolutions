namespace WB.Services.Export.Interview
{
    public class DoExportFileHeader
    {
        public DoExportFileHeader(string title, string description)
        {
            Title = title;
            Description = description;
        }

        public DoExportFileHeader(string title, string description, bool addCapture) : this(title, description)
        {
            AddCapture = addCapture;
        }

        public string Title { get; }
        public string Description { get; }
        public bool AddCapture { get; }
    }
}
