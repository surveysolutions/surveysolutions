using Microsoft.AspNetCore.Mvc;

namespace WB.UI.Designer.Code
{
    public static class ControllerExtensions
    {
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
