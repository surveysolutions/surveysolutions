namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser
{
    public class PreloadedContentMetaData
    {
        public PreloadedContentMetaData(string id, string title, PreloadedFileMetaData[] filesMetaInformation, AssignmentImportType assignmentImportType)
        {
            this.AssignmentImportType = assignmentImportType;
            this.Id = id;
            this.Title = title;
            this.FilesMetaInformation = filesMetaInformation;
        }

        public string Id { get; private set; }
        public string Title { get; private set; }
        public PreloadedFileMetaData[] FilesMetaInformation { get; private set; }
        public AssignmentImportType AssignmentImportType { get; private set; }
    }
}
