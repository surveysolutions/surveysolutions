using System.Threading.Tasks;
using Refit;

namespace WB.Core.BoundedContexts.Headquarters.DataExport
{
    public interface IExternalStoragesApi
    {
        [Post("/token")]
        Task<ExternalStorageTokenResponse> GetTokensByAuthorizationCodeAsync([Body(BodySerializationMethod.UrlEncoded)] ExternalStorageAccessTokenRequest request);
        [Post("/token")]
        Task<ExternalStorageTokenResponse> GetAccessTokenByRefreshTokenAsync([Body(BodySerializationMethod.UrlEncoded)] ExternalStorageRefreshTokenRequest request);
    }
    
    public class ExternalStorageAccessTokenRequest
    {
        public string code { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string redirect_uri { get; set; }
        public string grant_type { get; set; }
    }
    
    public class ExternalStorageRefreshTokenRequest
    {
        public string refresh_token { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string grant_type { get; set; }
    }

    public class ExternalStorageTokenResponse
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
        public string scope { get; set; }
        public string refresh_token { get; set; }
    }
}
