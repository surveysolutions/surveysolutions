using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.TabletInformation
{
    public class TabletInformationView
    {
        public string UserName { get; set; }

        public string UserId { get; set; }

        public string PackageName { get; set; }

        public string AndroidId { get; set; }

        public DateTime CreationDate { get; set; }

        public long Size { get; set; }

        public string DownloadUrl { get; set; }
    }
}
