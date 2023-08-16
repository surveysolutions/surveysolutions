namespace WB.UI.Headquarters.Models.Maps
{
    public class MapsModel
    {
        public string DataUrl { get; set; }
        public string UploadMapUrl { get; set; }
        public string UserMapLinkingUrl { get; set; }
        public string DeleteMapLinkUrl { get; set; }

        public bool IsObserver { get; set; }
        public bool IsObserving { get; set; }
        public string UploadMapsFileUrl { get; set; }
        public string UserMapsUrl { get; set; }
        public bool IsSupervisor { get; set; }
    }
}