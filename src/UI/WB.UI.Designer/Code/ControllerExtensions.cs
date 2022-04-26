using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace WB.UI.Designer.Code
{
    public static class ControllerExtensions
    {
        public static IActionResult ErrorWithReasonPhraseForHQ(this ControllerBase controller, int statusCode, string message)
        {
            // HTTP/2 doesn't have reason phrase, it for compatibility, if HQ on first version
            var feature = controller.Response.HttpContext.Features.Get<IHttpResponseFeature>();
            if(feature != null)
                feature.ReasonPhrase = message;
            return new JsonResult(new { message }) { StatusCode = statusCode };
        }
        
        public static IActionResult Error(this ControllerBase controller, int statusCode, string message)
        {
            return new JsonResult(new { message }) { StatusCode = statusCode };
        }

        public static IActionResult Error(this ControllerBase controller, int statusCode, object message)
        {
            return new JsonResult(message) { StatusCode = statusCode };
        }
    }
}
