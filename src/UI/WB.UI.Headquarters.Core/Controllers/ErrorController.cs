using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Exceptional;
using WB.UI.Headquarters.Code.Workspaces;
using WB.UI.Shared.Web.Attributes;
using WB.UI.Shared.Web.Exceptions;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [AllowDisabledWorkspaceAccess]
    [AllowPrimaryWorkspaceFallback]
    [NoTransaction]
    [Route("error")]
    public class ErrorController : Controller
    {
        private const int MaxLengthToSave = 1000;

        [Route("404")]
        public new IActionResult NotFound() => View("NotFound");

        [Route("401")]
        public IActionResult AccessDenied() => View("AccessDenied");

        [Route("403")]
        public IActionResult Forbidden() => this.View("Forbidden");

        [Route("500")]
        public IActionResult UnhandledException() => this.View("UnhandledException");

        [Route("{statusCode}")]

        public IActionResult Error(int? statusCode = null)
        {
            if (statusCode.HasValue)
            {
                switch (statusCode.Value)
                {
                    case 401: return AccessDenied();
                    case 403: return Forbidden();
                    case 404: return NotFound();
                    case 500: return UnhandledException();
                }
            }
            return UnhandledException();
        }
        
        [Route("AntiForgery")]
        public IActionResult AntiForgery() => this.View("AntiForgery");
        
        [Route("report")]
        [HttpPost]
        public IActionResult LogError([FromBody] JObject clientError)
        {
            string json = clientError.ToString();
            ClientException exception;
            
            try
            {
                var clientErrorModel = JsonConvert.DeserializeObject<ClientErrorModel>(json);
                exception = clientErrorModel != null
                    ? new ClientException(clientErrorModel)
                    : new ClientException("Client error is null", 
                        json.Substring(0, Math.Min(MaxLengthToSave, json.Length)));
            }
            catch
            {
                exception = new ClientException("Error deserialize client error", 
                    json.Substring(0, Math.Min(MaxLengthToSave, json.Length)));
            }
            
            exception.Log(HttpContext, category: "vue3");
            return Ok();
        }
    }
}
