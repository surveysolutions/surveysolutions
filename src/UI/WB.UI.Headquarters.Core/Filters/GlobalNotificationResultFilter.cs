using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Shared.Web.Attributes;

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
                if (filterContext.Filters.OfType<NoTransactionAttribute>().Any()) return;

                var workspace = filterContext.HttpContext.RequestServices.GetWorkspaceContext();

                if (workspace == null || workspace.IsSpecialWorkspace()) return;
                
                //respect scope
                var plainKeyValueStorage = filterContext.HttpContext.RequestServices.GetRequiredService<IPlainKeyValueStorage<GlobalNotice>>();

                var globalNotice = plainKeyValueStorage.GetById(AppSetting.GlobalNoticeKey);
                ((Controller)filterContext.Controller).ViewBag.GlobalNotice = 
                    string.IsNullOrEmpty(globalNotice?.Message) ? null : globalNotice.Message;
            }
        }
    }
}
