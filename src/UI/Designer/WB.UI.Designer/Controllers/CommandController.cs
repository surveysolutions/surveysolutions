using Main.Core.Commands.Questionnaire.Base;
using WB.UI.Designer.Utils;

namespace WB.UI.Designer.Controllers
{
    using System;
    using System.Web.Mvc;

    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.ServiceModel;

    using WB.Core.SharedKernel.Logger;
    using WB.UI.Designer.Code.Helpers;
    using WB.UI.Designer.Exceptions;

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
                    this.logger.Error(e);
                    error =
                        string.Format(
                            "Unexpected error occurred. Please contact support via following email: <a href=\"mailto:{0}\">{0}</a>",
                            AppSettings.Instance.AdminEmail);
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