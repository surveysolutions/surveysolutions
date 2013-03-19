using System.Net;

namespace WB.UI.Designer.Controllers
{
    using System;
    using System.Web.Mvc;

    using Main.Core.Commands.Questionnaire.Group;

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
                return Json(new { error = e.Message });
            }

            try
            {
                //this.commandService.Execute(concreteCommand);
            }
            catch (Exception e)
            {
                this.SetErrorStatusCode();
                if (e.InnerException is ArgumentException)
                {
                    var error = (ArgumentException) e.InnerException;
                    return Json(new { error = error.Message });
                }
                else
                {
                    return Json(new { error = e.Message });
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