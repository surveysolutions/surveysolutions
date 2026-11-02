using System;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
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
            public string ClientSecret { get; set; }
            public string AuthorizationUri { get; set; }
            public string TokenUri { get; set; }
            public string Scope { get; set; }

            public Uri GetTokenEndpointUri()
            {
                if (!Uri.TryCreate(this.TokenUri, UriKind.Absolute, out var tokenUri))
                    throw new InvalidOperationException("External storage token URI must be an absolute URI.");

                var uriBuilder = new UriBuilder(tokenUri);
                var path = uriBuilder.Path.TrimEnd('/');
                if (!path.EndsWith("/token", StringComparison.OrdinalIgnoreCase))
                    path += "/token";

                uriBuilder.Path = path;
                return uriBuilder.Uri;
            }
        }

        public OAuth2Settings OAuth2 { get; set; }
    }
}
