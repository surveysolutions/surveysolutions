namespace WB.Core.BoundedContexts.Headquarters.Implementation.ReadSide
{
    public class ReadSideEventHandlerDescription
    {
        public ReadSideEventHandlerDescription(string name,string[] usesViews, string[] buildsViews, bool supportsPartialRebuild)
        {
            this.SupportsPartialRebuild = supportsPartialRebuild;
            this.Name = name;
            this.UsesViews = usesViews;
            this.BuildsViews = buildsViews;
        }

        public string Name { get; private set; }
        public bool SupportsPartialRebuild { get; private set; }
        public string[] UsesViews { get; private set; }
        public string[] BuildsViews { get; private set; }
    }
}
