using System;

namespace WB.Core.SharedKernels.Enumerator.Services.MapService
{
    public class MapDescription
    {
        public MapDescription(MapType mapType, string mapName)
        {
            this.MapType = mapType;
            this.MapName = mapName;
        }

        public MapType MapType { set; get; }

        public string MapName { set; get; }
        public string MapFileName { set; get; }
        public string MapFullPath { set; get; }
        public long Size { set; get; }
        public DateTime CreationDate { set; get; }
    }

    public enum MapType
    {
        Unknown = 0,
        LocalFile = 1,
        OnlineImagery = 2,
        OnlineImageryWithLabels = 3,
        OnlineOpenStreetMap = 4
    }
}
