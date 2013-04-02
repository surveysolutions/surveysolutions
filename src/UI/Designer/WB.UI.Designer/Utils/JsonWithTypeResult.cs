using System;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Converters;

namespace WB.UI.Designer.Utils
{
    public class JsonWithTypeResult : JsonResult
    {
        public JsonWithTypeResult()
        {
            JsonRequestBehavior = JsonRequestBehavior.AllowGet;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            var response = context.HttpContext.Response;

            response.ContentType = !string.IsNullOrEmpty(this.ContentType) ? this.ContentType : "application/json";

            if (this.ContentEncoding != null)
            {
                response.ContentEncoding = this.ContentEncoding;
            }

            if (this.Data == null) return;

            response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(this.Data, Newtonsoft.Json.Formatting.None,
                new StringEnumConverter()));
        }
    }
}