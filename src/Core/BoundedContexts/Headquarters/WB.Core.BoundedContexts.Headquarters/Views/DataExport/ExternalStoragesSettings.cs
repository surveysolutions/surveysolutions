namespace WB.Core.BoundedContexts.Headquarters.Views.DataExport
{
    public class FakeExternalStoragesSettings : ExternalStoragesSettings
    {

    }

    public class ExternalStoragesSettings
    {
        public class OAuth2Settings
        {
            public string RedirectUri { get; set; }
            public string ResponseType { get; set; }
            public ExternalStorageOAuth2Settings Dropbox { get; set; }
            public ExternalStorageOAuth2Settings OneDrive { get; set; }
            public ExternalStorageOAuth2Settings GoogleDrive { get; set; }
        }

        public class ExternalStorageOAuth2Settings
        {
            public string ClientId { get; set; }
            public string AuthorizationUri { get; set; }
            public string Scope { get; set; }
        }

        public OAuth2Settings OAuth2 { get; set; }
    }
}
