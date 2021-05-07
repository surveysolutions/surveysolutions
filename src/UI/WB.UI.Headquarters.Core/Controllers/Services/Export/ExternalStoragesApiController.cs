using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Refit;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.UI.Headquarters.Code.Authentication;

namespace WB.UI.Headquarters.Controllers.Services.Export
{
    [Route("api/export/v1/externalstorages")]
    [Microsoft.AspNetCore.Authorization.Authorize(AuthenticationSchemes = AuthType.TenantToken)]
    public class ExternalStoragesApiController : Controller
    {
        private readonly ExternalStoragesSettings externalStoragesSettings;

        public ExternalStoragesApiController(ExternalStoragesSettings externalStoragesSettings)
        {
            this.externalStoragesSettings = externalStoragesSettings;
        }
        
        [Route("refreshtoken")]
        [HttpPost]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<string>> GetAccessTokenAsync(ExternalStorageType type, string refreshToken)
        {
            var response = await this.GetAccessTokenByRefreshTokenAsync(type, refreshToken);

            if(response.IsSuccessStatusCode)
                return this.Ok(response.Content?.AccessToken);
            else
            {
                return BadRequest($"Could not get access token");
            }
        }
        
        private async Task<ApiResponse<ExternalStorageTokenResponse>> GetAccessTokenByRefreshTokenAsync(ExternalStorageType type, string refreshToken)
        {
            var storageSettings = this.GetExternalStorageSettings(type);
            var client =  RestService.For<IOAuth2Api>(new HttpClient()
            {
                BaseAddress = new Uri(storageSettings.TokenUri)
            });
            var request = new ExternalStorageRefreshTokenRequest
            {
                RefreshToken = refreshToken,
                ClientId = storageSettings.ClientId,
                ClientSecret = storageSettings.ClientSecret,
                GrantType = "refresh_token"
            };

            return await client.GetAccessTokenByRefreshTokenAsync(request);
        }

        private ExternalStoragesSettings.ExternalStorageOAuth2Settings GetExternalStorageSettings(ExternalStorageType type)
        {
            switch (type)
            {
                case ExternalStorageType.Dropbox:
                    return this.externalStoragesSettings.OAuth2.Dropbox;
                case ExternalStorageType.GoogleDrive :
                    return this.externalStoragesSettings.OAuth2.GoogleDrive;
                case ExternalStorageType.OneDrive:
                    return this.externalStoragesSettings.OAuth2.OneDrive;
                default:
                    throw new NotSupportedException($"<{type}> not supported external storage type");
            }
        }
    }
}
