using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;

namespace WB.UI.Shared.Web.Filters
{
    public class UnderConstructionHttpFilter : System.Web.Http.Filters.ActionFilterAttribute
    {
        public override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var status = ServiceLocator.Current.GetInstance<UnderConstructionInfo>();

            if (status.Status == UnderConstructionStatus.Running)
            {
                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(status.Message ?? "Server Initializing. Please wait..."),
                };
                return Task.CompletedTask;
            }

            return base.OnActionExecutingAsync(actionContext, cancellationToken);
        }
    }

    public class UnderConstructionMvcFilter : System.Web.Mvc.ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var status = ServiceLocator.Current.GetInstance<UnderConstructionInfo>();

            if (status.Status == UnderConstructionStatus.Running)
            {
                filterContext.Result = new ContentResult()
                {
                    Content = GeneratePageWithMessage(status.Message ?? "Server Initializing. Please wait..."),
                    ContentType = "text/html"
                };
                return;
            }

            base.OnActionExecuting(filterContext); 
        }

        public string GeneratePageWithMessage(string message)
        {
            var title = "Server Initializing";
            var html = @"<!DOCTYPE html>
<html>
<head>
    <title>" + title + @"</title>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <meta http-equiv='refresh' content='30' />
</head>
<body>
    <div id='page' class='container-fluid'>
        <div id='main'>
            <h2>"
            + message +
          @"</h2>
        </div>
    </div>
    <div class='row-fluid' id='footer-block'>
    </div>
    <script type='text/javascript'>
        setTimeout(function() { document.location.reload(true); }, 1000 * 30);
    </script>
</body>
</html>";
            return html;
        }
    }
}
