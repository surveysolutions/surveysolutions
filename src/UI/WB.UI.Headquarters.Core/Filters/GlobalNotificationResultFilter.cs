using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.Headquarters.Filters
{
    public class GlobalNotificationResultFilter : IResultFilter
    {
        public void OnResultExecuted(ResultExecutedContext context)
        {
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext.Result is ViewResult)
            {
                //respect scope
                var plainKeyValueStorage = filterContext.HttpContext.RequestServices.GetRequiredService<IPlainKeyValueStorage<GlobalNotice>>();

                var globalNotice = plainKeyValueStorage.GetById(AppSetting.GlobalNoticeKey);
                ((Controller)filterContext.Controller).ViewBag.GlobalNotice = 
                    string.IsNullOrEmpty(globalNotice?.Message) ? null : globalNotice.Message;
            }
        }
    }
}
