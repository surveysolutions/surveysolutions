using System;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;

namespace WB.UI.Headquarters.Models.Maps
{
    public class MapDetailsModel
    {
        public string Id { get; set; }

        public double Size { get; set; }
        public string FileName { get; set; }
        public DateTime? ImportDate { get; set; }
        public string UploadedBy { get; set; }
        public double XMaxVal { set; get; }
        public double YMaxVal { set; get; }
        public double XMinVal { set; get; }
        public double YMinVal { set; get; }
        public int Wkid { set; get; }

        public double MaxScale { set; get; }
        public double MinScale { set; get; }
        public string ShapeType { get; set; }
        public int? ShapesCount { get; set; }

        public string DataUrl { get; set; }
        public string MapPreviewUrl { get; set; }
        public string MapsUrl { get; set; }
        public string DeleteMapUserLinkUrl { get; set; }
        public DuplicateLabelModel[] DuplicateMapLabels { get; set; }
        public bool IsPreviewGeoJson { get; set; }
        
        public ApiEndpoints Api { get; set; }

        public class ApiEndpoints
        {
            public string Users { get; set; }
        }
    }

    public class DuplicateLabelModel
    {
        public string Label { get; set; }
        public int Count { get; set; }
    }
}
