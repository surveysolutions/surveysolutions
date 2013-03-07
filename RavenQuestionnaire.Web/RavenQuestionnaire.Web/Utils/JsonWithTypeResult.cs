using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace RavenQuestionnaire.Web.Utils
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

            var serializer = new JavaScriptSerializer(new TypeResolver());
            response.Write(serializer.Serialize(this.Data));
        }
    }
}