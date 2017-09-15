using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.Headquarters.API
{
    [Authorize(Roles = "Administrator")]
    public class AdminSettingsController : ApiController
    {
        public class GlobalNoticeModel
        {
            public string Message { get; set; }
        }

        private readonly IPlainKeyValueStorage<GlobalNotice> appSettingsStorage;
        public AdminSettingsController(IPlainKeyValueStorage<GlobalNotice> appSettingsStorage)
        {
            if (appSettingsStorage == null) throw new ArgumentNullException(nameof(appSettingsStorage));
            this.appSettingsStorage = appSettingsStorage;
        }

        public HttpResponseMessage Get()
        {
            var globalNotice = this.appSettingsStorage.GetById(GlobalNotice.GlobalNoticeKey);
            if (globalNotice == null)
            {
                return Request.CreateResponse(new GlobalNoticeModel());
            }

            return Request.CreateResponse(new GlobalNoticeModel {Message = globalNotice.Message});
        }

        public HttpResponseMessage Post([FromBody] GlobalNoticeModel message)
        {
            if (string.IsNullOrEmpty(message?.Message))
            {
                this.appSettingsStorage.Remove(GlobalNotice.GlobalNoticeKey);
            }
            else
            {
                var globalNotice = this.appSettingsStorage.GetById(GlobalNotice.GlobalNoticeKey) ?? new GlobalNotice();
                globalNotice.Message = message.Message.Length > 1000 ? message.Message.Substring(0, 1000) : message.Message;
                this.appSettingsStorage.Store(globalNotice, GlobalNotice.GlobalNoticeKey);
            }

            return Request.CreateResponse(HttpStatusCode.OK, new {sucess = true});
        }
    }
}