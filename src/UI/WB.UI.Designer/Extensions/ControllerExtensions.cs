using System;
using Microsoft.AspNetCore.Mvc;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Designer.Extensions
{
    public static class ControllerExtensions
    {
        public static void Error(this Controller controller, string message, bool append = false) => WriteToTempData(controller,Alerts.ERROR, message, append);
        public static void Info(this Controller controller,string message, bool append = false) => WriteToTempData(controller,Alerts.INFORMATION, message, append);
        public static void Success(this Controller controller,string message, bool append = false) => WriteToTempData(controller,Alerts.SUCCESS, message, append);
        public static void Attention(this Controller controller, string message, bool append = false) => WriteToTempData(controller,Alerts.ATTENTION, message, append);

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
    }
}
