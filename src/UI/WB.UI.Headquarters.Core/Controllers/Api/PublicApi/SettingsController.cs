using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using Swashbuckle.Swagger.Annotations;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.PublicApi
{
    /// <summary>
    /// Provides a methods for managing settings
    /// </summary>
    [ApiBasicAuth(UserRoles.ApiUser, UserRoles.Administrator, TreatPasswordAsPlain = true)]
    [RoutePrefix(@"api/v1/settings")]
    [SwaggerResponseRemoveDefaults]
    public class SettingsController : ApiController
    {
        private readonly IPlainKeyValueStorage<GlobalNotice> appSettingsStorage;

        public SettingsController(IPlainKeyValueStorage<GlobalNotice> appSettingsStorage)
        {
            this.appSettingsStorage = appSettingsStorage ?? throw new ArgumentNullException(nameof(appSettingsStorage));
        }

        /// <summary>
        /// Set global notice for the headquarters application
        /// </summary>
        [Route("globalnotice")]
        [SwaggerResponse(204, "Global notice set")]
        [SwaggerResponse(400, "Message text missing")]
        public HttpResponseMessage PutGlobalNotice([FromBody]SetGlobalNoticeApiModel request)
        {
            if (string.IsNullOrEmpty(request?.Message)) return Request.CreateResponse(HttpStatusCode.BadRequest);

            this.appSettingsStorage.Store(new GlobalNotice { Message = request.Message }, AppSetting.GlobalNoticeKey);
            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Remove global notice for the headquarters application
        /// </summary>
        [Route("globalnotice")]
        [SwaggerResponse(204, "Global notice removed")]
        public HttpResponseMessage DeleteGlobalNotice()
        {
            this.appSettingsStorage.Remove(AppSetting.GlobalNoticeKey);
            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Get global notice for the headquarters application
        /// </summary>
        [Route("globalnotice")]
        [SwaggerResponse(200, "Gets current global notice for the headquarters application", typeof(GlobalNoticeApiView))]
        public HttpResponseMessage GetGlobalNotice()
        {
            var globalNotice = this.appSettingsStorage.GetById(AppSetting.GlobalNoticeKey) ?? new GlobalNotice();

            return Request.CreateResponse(HttpStatusCode.OK,
                new GlobalNoticeApiView
                {
                    Message = globalNotice.Message
                });
        }
    }
}
