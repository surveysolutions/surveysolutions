using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Headquarters.Models.DataExport
{
    public class ExportModel
    {
        public IEnumerable<TemplateViewItem> Questionnaires { get; set; }
        public List<InterviewStatus> ExportStatuses { get; set; }
        public ExternalStoragesSettings ExternalStoragesSettings { get; set; }
    }

    public class NewExportModel
    {
        public dynamic Api { get; set; }
        public ComboboxViewItem[] Statuses { get; set; }
        public ExternalStoragesPublicSettings ExternalStoragesSettings { get; set; }
    }

    public class ExternalStoragesPublicSettings
    {
        public class OAuth2PublicSettings
        {
            public string RedirectUri { get; set; }
            public string ResponseType { get; set; }
            public ExternalStorageOAuth2PublicSettings Dropbox { get; set; }
            public ExternalStorageOAuth2PublicSettings OneDrive { get; set; }
            public ExternalStorageOAuth2PublicSettings GoogleDrive { get; set; }
        }

        public class ExternalStorageOAuth2PublicSettings
        {
            public string ClientId { get; set; }
            public string AuthorizationUri { get; set; }
            public string Scope { get; set; }
        }

        public OAuth2PublicSettings OAuth2 { get; set; }
    }
}
