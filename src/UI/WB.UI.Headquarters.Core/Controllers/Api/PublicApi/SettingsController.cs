#nullable enable
using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Swashbuckle.AspNetCore.Annotations;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi
{
    /// <summary>
    /// Provides a methods for managing settings
    /// </summary>
    [Authorize(Roles = "ApiUser, Administrator")]
    [Route(@"api/v1/settings")]
    [Localizable(false)]
    [PublicApiJson]
    public class SettingsController : ControllerBase
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
        [HttpPut]
        [SwaggerResponse(204, "Global notice set")]
        [SwaggerResponse(400, "Message text missing", type: typeof(ValidationProblemDetails))]
        public ActionResult PutGlobalNotice([FromBody, BindRequired]SetGlobalNoticeApiModel request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem();
            
            if (string.IsNullOrEmpty(request.Message))
                return BadRequest();

            this.appSettingsStorage.Store(new GlobalNotice { Message = request.Message }, AppSetting.GlobalNoticeKey);
            return NoContent();
        }

        /// <summary>
        /// Remove global notice for the headquarters application
        /// </summary>
        [Route("globalnotice")]
        [SwaggerResponse(204, "Global notice removed")]
        [HttpDelete]
        public ActionResult DeleteGlobalNotice()
        {
            this.appSettingsStorage.Remove(AppSetting.GlobalNoticeKey);
            return NoContent();
        }

        /// <summary>
        /// Get global notice for the headquarters application
        /// </summary>
        [Route("globalnotice")]
        [HttpGet]
        [SwaggerResponse(200, "Gets current global notice for the headquarters application", typeof(GlobalNoticeApiView))]
        public ActionResult<GlobalNoticeApiView> GetGlobalNotice()
        {
            var globalNotice = this.appSettingsStorage.GetById(AppSetting.GlobalNoticeKey) ?? new GlobalNotice();

            return new GlobalNoticeApiView
            {
                Message = globalNotice.Message
            };
        }
    }
}
