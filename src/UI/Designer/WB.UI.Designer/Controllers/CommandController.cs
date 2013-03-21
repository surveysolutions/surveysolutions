using System.Net;

namespace WB.UI.Designer.Controllers
{
    using System;
    using System.Web.Mvc;

    using Elmah;

    using Main.Core.Commands.Questionnaire.Group;
    using Main.Core.Domain;

    using NLog;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.ServiceModel;

    using Newtonsoft.Json.Linq;

    using WB.UI.Designer.Code.Helpers;

    public class CommandController : Controller
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
            catch (CommandDeserializationException e)
            {
                Logger.ErrorException(string.Format("Failed to deserialize command of type '{0}':\r\n{1}", type, command), e);

                this.SetErrorStatusCode();
                return this.Json(new { error = "Unexpected error occurred: " + e.Message });
            }

            try
            {
                this.commandService.Execute(concreteCommand);
            }
            catch (Exception e)
            {
                if (e.InnerException is DomainException)
                {
                    this.SetErrorStatusCode();
                    return this.Json(new { error = e.InnerException.Message });
                }
                else
                {
                    throw;
                }
            }

            return this.Json(new { });
        }

        private void SetErrorStatusCode()
        {
            this.Response.StatusCode = (int) HttpStatusCode.BadRequest;
        }
    }
}