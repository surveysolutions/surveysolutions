using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.UI.Shared.Web;
using WB.UI.Shared.Web.CommandDeserialization;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Controllers
{
    using System;
    using System.Web.Mvc;
    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.ServiceModel;

    using WB.UI.Designer.Utils;

    [CustomAuthorize]
    public class CommandController : Controller
    {
        private readonly IMembershipUserService userHelper;
        private readonly ICommandService commandService;
        private readonly ICommandDeserializer commandDeserializer;
        private readonly IExpressionReplacer expressionReplacer;
        private readonly ILogger logger;

        public CommandController(ICommandService commandService, ICommandDeserializer commandDeserializer,
            IExpressionReplacer expressionReplacer, IMembershipUserService userHelper)
        {
            this.userHelper = userHelper;
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
                this.SetResponsible(concreteCommand);
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

        private void SetResponsible(ICommand concreteCommand)
        {
            if (concreteCommand is QuestionnaireCommandBase)
            {
                (concreteCommand as QuestionnaireCommandBase).ResponsibleId = userHelper.WebUser.UserId;
            }
        }

        private void ReplaceStataCaptionsWithGuidsIfNeeded(ICommand command)
        {

            var questionCommand = command as FullQuestionDataCommand;
            if (questionCommand != null)
            {
                questionCommand.Condition = this.expressionReplacer.ReplaceStataCaptionsWithGuids(questionCommand.Condition, questionCommand.QuestionnaireId);
                questionCommand.ValidationExpression = this.expressionReplacer.ReplaceStataCaptionsWithGuids(questionCommand.ValidationExpression, questionCommand.QuestionnaireId);
             }
            // can command be FullGroupDataCommand and FullQuestionDataCommand at the same time?
            // if not this cast is unnecessary 
            var newGroupCommand = command as FullGroupDataCommand;
            if (newGroupCommand != null)
            {
                newGroupCommand.Condition = this.expressionReplacer.ReplaceStataCaptionsWithGuids(newGroupCommand.Condition, newGroupCommand.QuestionnaireId);
             } 
         }
    }
}