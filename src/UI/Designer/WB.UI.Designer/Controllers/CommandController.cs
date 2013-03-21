using System.Net;

namespace WB.UI.Designer.Controllers
{
    using System;
    using System.Web.Mvc;

    using Elmah;

    using Main.Core.Commands.Questionnaire.Group;
    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.ServiceModel;

    using Newtonsoft.Json.Linq;

    using WB.UI.Designer.Code.Helpers;

    public class CommandController : Controller
    {
        private readonly ICommandService commandService;
        private readonly ICommandDeserializer commandDeserializer;

        public CommandController(ICommandService commandService, ICommandDeserializer commandDeserializer)
        {
            this.commandService = commandService;
            this.commandDeserializer = commandDeserializer;
        }

        [HttpPost]
        public JsonResult Execute(string type, string command)
        {
            ICommand concreteCommand;
            try
            {
                concreteCommand = this.commandDeserializer.Deserialize(type, command);
            }
            catch (Exception e)
            {
                #warning TLK: register to Elmah or rethrow (better rethrow)
                return Json(new { error = e.Message });
            }

            try
            {
                this.commandService.Execute(concreteCommand);
            }
            catch (Exception e)
            {
                this.SetErrorStatusCode();
                if (e.InnerException is DomainException)
                {
                    return this.Json(new { error = e.InnerException.Message });
                }
                else
                {
                    #warning TLK: register to Elmah or rethrow (better rethrow)
                    return this.Json(new { error = e.Message });
                }
            }

            return Json(new { });
        }

        private void SetErrorStatusCode()
        {
            this.Response.StatusCode = (int) HttpStatusCode.BadRequest;
        }
    }
}