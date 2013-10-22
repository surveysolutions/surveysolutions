using System.Web.Security;
using Main.Core.Domain.Exceptions;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Models;
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
            var returnValue = new JsonQuestionnaireResult();
            try
            {
                var concreteCommand = this.commandDeserializer.Deserialize(type, command);
                this.SetResponsible(concreteCommand);
                this.ValidateAddSharedPersonCommand(concreteCommand);
                this.ValidateRemoveSharedPersonCommand(concreteCommand);
                this.ReplaceStataCaptionsWithGuidsIfNeeded(concreteCommand);
                this.commandService.Execute(concreteCommand);
            }
            catch (Exception e)
            {
                var domainEx = e.GetSelfOrInnerAs<DomainException>();
                if (domainEx == null)
                {
                    logger.Error(string.Format("Error on command of type ({0}) handling ", type), e);
                    throw;
                }
                else
                {
                    returnValue.IsSuccess = false;
                    returnValue.HasPermissions = domainEx.ErrorType != DomainExceptionType.DoesNotHavePermissionsForEdit;
                    returnValue.Error = domainEx.Message;
                }

            }

            return this.Json(returnValue);
        }

        private void SetResponsible(ICommand concreteCommand)
        {
            var currentCommand = concreteCommand as QuestionnaireCommandBase;
            if (currentCommand != null)
            {
                currentCommand.ResponsibleId = userHelper.WebUser.UserId;
            }
        }

        private void ReplaceStataCaptionsWithGuidsIfNeeded(ICommand command)
        {

            var questionCommand = command as AbstractQuestionCommand;
            if (questionCommand != null)
            {
                questionCommand.Condition = this.expressionReplacer.ReplaceStataCaptionsWithGuids(questionCommand.Condition, questionCommand.QuestionnaireId);
                questionCommand.ValidationExpression = this.expressionReplacer.ReplaceStataCaptionsWithGuids(questionCommand.ValidationExpression, questionCommand.QuestionnaireId);

                return;
            }
            
            var newGroupCommand = command as FullGroupDataCommand;
            if (newGroupCommand != null)
            {
                newGroupCommand.Condition = this.expressionReplacer.ReplaceStataCaptionsWithGuids(newGroupCommand.Condition, newGroupCommand.QuestionnaireId);
             } 
         }

        private void ValidateAddSharedPersonCommand(ICommand command)
        {
            var addSharedPersonCommand = command as AddSharedPersonToQuestionnaireCommand;
            if (addSharedPersonCommand != null)
            {
                var sharedPersonUserName = Membership.GetUserNameByEmail(addSharedPersonCommand.Email);

                addSharedPersonCommand.PersonId = Membership.GetUser(sharedPersonUserName).ProviderUserKey.AsGuid();
            }
        }

        private void ValidateRemoveSharedPersonCommand(ICommand command)
        {
            var removeSharedPersonCommand = command as RemoveSharedPersonFromQuestionnaireCommand;
            if (removeSharedPersonCommand != null)
            {
                var sharedPersonUserName = Membership.GetUserNameByEmail(removeSharedPersonCommand.Email);
                removeSharedPersonCommand.PersonId = Membership.GetUser(sharedPersonUserName).ProviderUserKey.AsGuid();
            }
        }
    }
}