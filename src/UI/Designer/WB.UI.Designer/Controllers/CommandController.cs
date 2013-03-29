using Main.Core.Commands.Questionnaire.Question;

using WB.UI.Designer.Utils;

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

    [CustomAuthorize]
    public class CommandController : Controller
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ICommandService commandService;
        private readonly ICommandDeserializer commandDeserializer;
        private readonly IExpressionReplacer expressionReplacer;

        public CommandController(ICommandService commandService, ICommandDeserializer commandDeserializer, IExpressionReplacer expressionReplacer)
        {
            this.commandService = commandService;
            this.commandDeserializer = commandDeserializer;
            this.expressionReplacer = expressionReplacer;
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

                return this.Json(new { error = "Unexpected error occurred: " + e.Message });
            }

            this.PrepareCommandForExecution(concreteCommand);

            try
            {
                this.commandService.Execute(concreteCommand);
            }
            catch (Exception e)
            {
                if (e.InnerException is DomainException)
                {
                    return this.Json(new { error = e.InnerException.Message });
                }
                else
                {
                    throw;
                }
            }

            return this.Json(new { });
        }

        private void PrepareCommandForExecution(ICommand command)
        {
            this.ReplaceStataCaptionsWithGuidsIfNeeded(command);
        }

        private void ReplaceStataCaptionsWithGuidsIfNeeded(ICommand command)
        {
            if (!(command is FullQuestionDataCommand))
                return;

            var questionCommand = (FullQuestionDataCommand) command;

            questionCommand.Condition = this.expressionReplacer.ReplaceStataCaptionsWithGuids(
                questionCommand.Condition, questionCommand.QuestionnaireId);

            questionCommand.ValidationExpression = this.expressionReplacer.ReplaceStataCaptionsWithGuids(
                questionCommand.ValidationExpression, questionCommand.QuestionnaireId);
        }
    }
}