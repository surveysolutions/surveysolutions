namespace WB.UI.Headquarters.Models.Maps
{
    public class UserMapLinkModel
    {
        public string DownloadAllUrl { get; set; }
        public string UploadUrl { get; set; }
        public string MapsUrl { get; set; }

        public bool IsObserver { get; set; }
        public bool IsObserving { get; set; }

        public string FileExtension { get; set; }
        public string UserMapsUrl { get; set; }
    }
}