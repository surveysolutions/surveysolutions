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
        internal const string settingsKey = "settings";

        public class GlobalNoticeModel
        {
            public string Message { get; set; }
        }

        private readonly IPlainKeyValueStorage<GlobalNotice> noticeStorage;
        public AdminSettingsController(IPlainKeyValueStorage<GlobalNotice> noticeStorage)
        {
            if (noticeStorage == null) throw new ArgumentNullException(nameof(noticeStorage));
            this.noticeStorage = noticeStorage;
        }

        public HttpResponseMessage Get()
        {
            var globalNotice = this.noticeStorage.GetById(settingsKey);
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
                this.noticeStorage.Remove(settingsKey);
            }
            else
            {
                var globalNotice = this.noticeStorage.GetById(settingsKey) ?? new GlobalNotice();
                globalNotice.Message = message.Message.Length > 1000 ? message.Message.Substring(0, 1000) : message.Message;
                this.noticeStorage.Store(globalNotice, settingsKey);
            }

            return Request.CreateResponse(HttpStatusCode.OK, new {sucess = true});
        }
    }
}