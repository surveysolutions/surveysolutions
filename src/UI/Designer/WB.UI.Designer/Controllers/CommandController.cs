using System.Web.Security;
using Main.Core.View;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Models;
using WB.UI.Designer.Views.Questionnaire;
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
        private readonly ILogger logger;

        private readonly IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory;
        public CommandController(ICommandService commandService, ICommandDeserializer commandDeserializer, IMembershipUserService userHelper, IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory)
        {
            this.userHelper = userHelper;
            this.commandService = commandService;
            this.commandDeserializer = commandDeserializer;
            this.logger = ServiceLocator.Current.GetInstance<ILogger>();
            this.questionnaireViewFactory = questionnaireViewFactory;
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
                var domainEx = e.GetSelfOrInnerAs<QuestionnaireException>();
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
                questionCommand.Condition = questionCommand.Condition;
                questionCommand.ValidationExpression = questionCommand.ValidationExpression;

                return;
            }

            var newGroupCommand = command as FullGroupDataCommand;
            if (newGroupCommand != null)
            {
                newGroupCommand.Condition = newGroupCommand.Condition;
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