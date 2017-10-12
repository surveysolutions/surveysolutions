namespace WB.Core.BoundedContexts.Headquarters.Maps
{
    public class MapViewItem
    {
        public string FileName { set; get; }
        public string ImportDate { get; set; }
        public long Size { get; set; }
        public double MaxScale { get; set; }
        public double MinScale { get; set; }
    }
}
