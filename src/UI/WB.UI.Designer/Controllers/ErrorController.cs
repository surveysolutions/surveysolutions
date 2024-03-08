using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Exceptional;
using WB.UI.Designer.Exceptions;
using WB.UI.Designer.Extensions;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace WB.UI.Designer.Controllers
{
    [Route("error")]
    public class ErrorController : Controller
    {
        [Route("500")]
        public ActionResult Index() => View("Index");
        [Route("404")]
        public new ActionResult NotFound() => View("NotFound");
        [Route("401")]
        public ActionResult AccessDenied() => View("AccessDenied");
        [Route("403")]
        public ActionResult Forbidden() => this.View("Forbidden");
        
        [Route("{statusCode:int}")]

        public IActionResult Error(int? statusCode = null)
        {
            if (statusCode.HasValue)
            {
                switch (statusCode.Value)
                {
                    case 401: return AccessDenied();
                    case 403: return Forbidden();
                    case 404: return NotFound();
                    case 500: return Index();
                }
            }
            return Index();
        }
        
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
                    : new ClientException("Client error is null", json);
            }
            catch
            {
                exception = new ClientException("Error deserialize client error", json);
            }
            
            exception.Log(null, category: "vue3");
            return Ok();
        }
    }
}
