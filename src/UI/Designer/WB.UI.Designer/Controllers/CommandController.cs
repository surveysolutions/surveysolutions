namespace WB.UI.Designer.Controllers
{
    using System;
    using System.Web.Mvc;

    using Main.Core.Commands.Questionnaire.Group;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.ServiceModel;

    using Newtonsoft.Json.Linq;

    public class CommandController : Controller
    {
        private readonly ICommandService commandService;

        public CommandController(ICommandService commandService)
        {
            this.commandService = commandService;
        }

        [HttpPost]
        public ActionResult Execute(string command)
        {
            dynamic parsedCommand = JObject.Parse(command);

            var concreteCommand = new NewUpdateGroupCommand(
                Guid.Parse((string)parsedCommand.questionnaireId), Guid.Parse((string)parsedCommand.groupId), (string)parsedCommand.title);

            this.commandService.Execute(concreteCommand);

            return new EmptyResult();
        }
    }
}