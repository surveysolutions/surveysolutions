using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Headquarters.API;

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
                var globalNotice = this.NoticeStorage.GetById(AdminSettingsController.settingsKey);
                viewResult.ViewBag.GlobalNotice = string.IsNullOrEmpty(globalNotice?.Message) ? null : globalNotice.Message;
            }
        }
    }
}
