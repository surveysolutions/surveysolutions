using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.Headquarters.Filters
{
    public class GlobalNotificationAttribute : ActionFilterAttribute
    {
        private IPlainKeyValueStorage<GlobalNotice> NoticeStorage => 
            ServiceLocator.Current.GetInstance<IPlainKeyValueStorage<GlobalNotice>>();

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            base.OnResultExecuting(filterContext);

            var viewResult = filterContext.Result as ViewResult;

            if (viewResult != null)
            {
                var globalNotice = this.NoticeStorage.GetById(GlobalNotice.GlobalNoticeKey);
                viewResult.ViewBag.GlobalNotice = string.IsNullOrEmpty(globalNotice?.Message) ? null : globalNotice.Message;
            }
        }
    }
}
