namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser
{
    public class PreloadedDataByFile
    {
        public PreloadedDataByFile(string fileName, string[] header, string[][] content)
        {
            this.FileName = fileName;
            this.Header = header;
            this.Content = content;
        }

        public string FileName { get; private set; }
        public string[] Header { get; private set; }
        public string[][] Content { get; private set; }
    }
}
