namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser
{
    public class PreloadedData
    {
        public PreloadedData(PreloadedDataByFile[] levels)
        {
            Levels = levels;
        }

        public PreloadedDataByFile[] Levels { get; }
    }
}