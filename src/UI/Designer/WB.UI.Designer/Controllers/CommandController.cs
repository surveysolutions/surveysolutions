using Main.Core.Commands.Questionnaire.Question;
using WB.UI.Designer.Utils;

namespace WB.UI.Designer.Controllers
{
    using System;
    using System.Web.Mvc;

    using Main.Core.Commands.Questionnaire.Group;
    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.ServiceModel;

    using WB.UI.Designer.Code.Helpers;
    using WB.UI.Designer.Exceptions;
    using WB.UI.Shared.Log;

    [CustomAuthorize]
    public class CommandController : Controller
    {
        private readonly ILog logger;
        private readonly ICommandService commandService;
        private readonly ICommandDeserializer commandDeserializer;
        private readonly IExpressionReplacer expressionReplacer;

        public CommandController(ICommandService commandService, ICommandDeserializer commandDeserializer, IExpressionReplacer expressionReplacer, ILog logger)
        {
            this.commandService = commandService;
            this.commandDeserializer = commandDeserializer;
            this.expressionReplacer = expressionReplacer;
            this.logger = logger;
        }

        [HttpPost]
        [CustomHandleErrorFilter]
        public JsonResult Execute(string type, string command)
        {
            ICommand concreteCommand;
            try
            {
                concreteCommand = this.commandDeserializer.Deserialize(type, command);
            }
            catch (CommandDeserializationException e)
            {
                this.logger.Error(string.Format("Failed to deserialize command of type '{0}':\r\n{1}", type, command), e);

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
                else if (e.InnerException!=null && e.InnerException.InnerException is DomainException)
                {
                    return this.Json(new { error = e.InnerException.InnerException.Message });
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
            if (command is FullQuestionDataCommand)
            {
                var questionCommand = (FullQuestionDataCommand)command;
                questionCommand.Condition = this.expressionReplacer.ReplaceStataCaptionsWithGuids(questionCommand.Condition, questionCommand.QuestionnaireId);
                questionCommand.ValidationExpression = this.expressionReplacer.ReplaceStataCaptionsWithGuids(questionCommand.ValidationExpression, questionCommand.QuestionnaireId);
             }
 
            if (command is FullGroupDataCommand)
            {
                var newGroupCommand = (FullGroupDataCommand)command;
                
                newGroupCommand.Condition = this.expressionReplacer.ReplaceStataCaptionsWithGuids(newGroupCommand.Condition, newGroupCommand.QuestionnaireId);
             } 
         }
    }
}