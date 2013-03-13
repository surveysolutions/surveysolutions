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
        public ActionResult Execute(string command)
        {
            ICommand concreteCommand = this.DeserializeCommand(command);

            this.commandService.Execute(concreteCommand);

            return new EmptyResult();
        }

        private ICommand DeserializeCommand(string serializedCommand)
        {
            return this.commandDeserializer.Deserialize(serializedCommand);
        }
    }
}