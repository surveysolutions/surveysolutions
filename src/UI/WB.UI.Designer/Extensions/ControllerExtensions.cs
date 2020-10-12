using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Designer.Extensions
{
    public static class ControllerExtensions
    {
        public static void Error(this Controller controller, string message, bool append = false) => WriteToTempData(controller,Alerts.ERROR, message, append);
        public static void Success(this Controller controller,string message, bool append = false) => WriteToTempData(controller,Alerts.SUCCESS, message, append);

        static void WriteToTempData(Controller controller, string key, string message, bool append = false)
        {
            if (controller.TempData.ContainsKey(key))
            {
                if (append)
                {
                    controller.TempData[key] += Environment.NewLine + message;
                }
                else
                {
                    controller.TempData[key] = message;
                }
            }
            else
            {
                controller.TempData.Add(key, message);
            }
        }
        
        public static HtmlString RenderConfig(this IHtmlHelper helper, object model)
        {
            var json = model != null ? JsonConvert.SerializeObject(model, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }) : "null";

            return new HtmlString($@"<script>window.CONFIG={json}</script>");
        }
    }
}
