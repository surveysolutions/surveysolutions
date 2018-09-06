using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.Headquarters.Filters
{
    public class GlobalNotificationAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            base.OnResultExecuting(filterContext);

            if (filterContext.Result is ViewResult viewResult)
            {
                //respect scope
                var plainKeyValueStorage = DependencyResolver.Current.GetService<IPlainKeyValueStorage<GlobalNotice>>();

                var globalNotice = plainKeyValueStorage.GetById(AppSetting.GlobalNoticeKey);
                viewResult.ViewBag.GlobalNotice = string.IsNullOrEmpty(globalNotice?.Message) ? null : globalNotice.Message;
            }
        }
    }
}
