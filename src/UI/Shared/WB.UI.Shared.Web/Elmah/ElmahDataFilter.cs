using System.Web;
using Elmah;

namespace WB.UI.Shared.Web.Elmah
{
    public static class ElmahDataFilter
    {
        public static void Apply(ExceptionFilterEventArgs e, HttpContext ctx)
        {
            ctx.Request.ServerVariables["ALL_HTTP"] = string.Empty;
            ctx.Request.ServerVariables["ALL_RAW"] = string.Empty;
            ctx.Request.ServerVariables["HTTP_COOKIE"] = string.Empty;
        }
    }
}
