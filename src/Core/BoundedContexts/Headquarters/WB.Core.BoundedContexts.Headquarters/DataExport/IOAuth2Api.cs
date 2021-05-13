using System.Threading.Tasks;
using Newtonsoft.Json;
using Refit;

namespace WB.Core.BoundedContexts.Headquarters.DataExport
{
    public interface IOAuth2Api
    {
        [Post("/token")]
        Task<ApiResponse<ExternalStorageTokenResponse>> GetTokensByAuthorizationCodeAsync([Body(BodySerializationMethod.UrlEncoded)] ExternalStorageAccessTokenRequest request);
        [Post("/token")]
        Task<ApiResponse<ExternalStorageTokenResponse>> GetAccessTokenByRefreshTokenAsync([Body(BodySerializationMethod.UrlEncoded)] ExternalStorageRefreshTokenRequest request);
    }
    
    public class ExternalStorageAccessTokenRequest
    {
        [AliasAs("code")]
        public string Code { get; set; }
        [AliasAs("client_id")]
        public string ClientId { get; set; }
        [AliasAs("client_secret")]
        public string ClientSecret { get; set; }
        [AliasAs("redirect_uri")]
        public string RedirectUri { get; set; }
        [AliasAs("grant_type")]
        public string GrantType { get; set; }
    }
    
    public class ExternalStorageRefreshTokenRequest
    {
        [AliasAs("refresh_token")]
        public string RefreshToken { get; set; }
        [AliasAs("client_id")]
        public string ClientId { get; set; }
        [AliasAs("client_secret")]
        public string ClientSecret { get; set; }
        [AliasAs("grant_type")]
        public string GrantType { get; set; }
    }

    public class ExternalStorageTokenResponse
    {
        [JsonProperty(PropertyName="access_token")]
        public string AccessToken { get; set; }
        [JsonProperty(PropertyName="expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty(PropertyName="token_type")]
        public string TokenType { get; set; }
        [JsonProperty(PropertyName="scope")]
        public string Scope { get; set; }
        [JsonProperty(PropertyName="refresh_token")]
        public string RefreshToken { get; set; }
    }
}
