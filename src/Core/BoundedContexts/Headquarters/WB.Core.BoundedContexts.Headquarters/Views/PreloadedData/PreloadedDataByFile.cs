using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Headquarters.Views.PreloadedData
{
    public class PreloadedDataByFile
    {
        public PreloadedDataByFile(string id, string fileName, string[] header, string[][] content)
        {
            this.Id = id;
            this.FileName = fileName;
            this.Header = header;
            this.Content = content;
        }

        public string Id { get; private set; }
        public string FileName { get; private set; }
        public string[] Header { get; private set; }
        public string[][] Content { get; private set; }
    }
}
