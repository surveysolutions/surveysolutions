using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.UI.Shared.Web;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.UI.Designer.Controllers
{
    using System;
    using System.Web.Mvc;
    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.ServiceModel;

    using WB.UI.Designer.Code.Helpers;
    using WB.UI.Designer.Exceptions;
    using WB.UI.Designer.Utils;

    [CustomAuthorize]
    public class CommandController : Controller
    {
        private readonly ICommandService commandService;
        private readonly ICommandDeserializer commandDeserializer;
        private readonly IExpressionReplacer expressionReplacer;
        private readonly ILogger logger;

        public CommandController(ICommandService commandService, ICommandDeserializer commandDeserializer, IExpressionReplacer expressionReplacer)
        {
            this.commandService = commandService;
            this.commandDeserializer = commandDeserializer;
            this.expressionReplacer = expressionReplacer;
            this.logger = ServiceLocator.Current.GetInstance<ILogger>();
        }

        [HttpPost]
        public JsonResult Execute(string type, string command)
        {
            string error = string.Empty;
            try
            {
                var concreteCommand = this.commandDeserializer.Deserialize(type, command);
                this.ReplaceStataCaptionsWithGuidsIfNeeded(concreteCommand);
                this.commandService.Execute(concreteCommand);
            }
            catch (Exception e)
            {
                var domainEx = e.As<DomainException>();
                if (domainEx == null)
                {
                    throw e;
                }
                else
                {
                    error = domainEx.Message;
                }

            }

            return this.Json(string.IsNullOrEmpty(error) ? (object)new { } : new { error = error });
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