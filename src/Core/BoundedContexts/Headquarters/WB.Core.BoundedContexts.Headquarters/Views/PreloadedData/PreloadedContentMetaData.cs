namespace WB.Core.BoundedContexts.Headquarters.Views.PreloadedData
{
    public class PreloadedContentMetaData
    {
        public PreloadedContentMetaData(string id, string title, PreloadedFileMetaData[] filesMetaInformation, PreloadedContentType preloadedContentType)
        {
            this.PreloadedContentType = preloadedContentType;
            this.Id = id;
            this.Title = title;
            this.FilesMetaInformation = filesMetaInformation;
        }

        public string Id { get; private set; }
        public string Title { get; private set; }
        public PreloadedFileMetaData[] FilesMetaInformation { get; private set; }
        public PreloadedContentType PreloadedContentType { get; private set; }
    }
}
