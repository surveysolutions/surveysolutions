using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.Headquarters.Filters
{
    public class GlobalNotificationAttribute : ActionFilterAttribute
    {
        private readonly IPlainKeyValueStorage<GlobalNotice> plainKeyValueStorage;

        public GlobalNotificationAttribute(IPlainKeyValueStorage<GlobalNotice> plainKeyValueStorage)
        {
            this.plainKeyValueStorage = plainKeyValueStorage;
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            base.OnResultExecuting(filterContext);

            if (filterContext.Result is ViewResult viewResult)
            {
                var globalNotice = this.plainKeyValueStorage.GetById(AppSetting.GlobalNoticeKey);
                viewResult.ViewBag.GlobalNotice = string.IsNullOrEmpty(globalNotice?.Message) ? null : globalNotice.Message;
            }
        }
    }
}
