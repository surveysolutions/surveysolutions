namespace WB.Core.BoundedContexts.Headquarters.Implementation
{
    public class MapFileDescription
    {
        public MapFileDescription(string name, long size)
        {
            this.FileName = name;
            this.FileSize = size;
        }

        public string FileName { get; private set; }
        public long FileSize { get; private set; }
    }
}
