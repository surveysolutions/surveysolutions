using System.Web.Security;
using Ncqrs.Commanding;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.UI.Designer.Extensions;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Code.Implementation
{
    public class CommandPreprocessor : ICommandPreprocessor
    {
        private readonly IMembershipUserService userHelper;

        public CommandPreprocessor(IMembershipUserService userHelper)
        {
            this.userHelper = userHelper;
        }

        public void PrepareDeserializedCommandForExecution(ICommand command)
        {
            this.SetResponsible(command);
            ValidateAddSharedPersonCommand(command);
            ValidateRemoveSharedPersonCommand(command);
            ReplaceStataCaptionsWithGuidsIfNeeded(command);
        }

        private void SetResponsible(ICommand command)
        {
            var currentCommand = command as QuestionnaireCommandBase;
            if (currentCommand != null)
            {
                currentCommand.ResponsibleId = this.userHelper.WebUser.UserId;
            }
        }

        private static void ReplaceStataCaptionsWithGuidsIfNeeded(ICommand command)
        {
            var questionCommand = command as AbstractQuestionCommand;
            if (questionCommand != null)
            {
                questionCommand.EnablementCondition = questionCommand.EnablementCondition;
                questionCommand.ValidationExpression = questionCommand.ValidationExpression;

                return;
            }

            var newGroupCommand = command as FullGroupDataCommand;
            if (newGroupCommand != null)
            {
                newGroupCommand.Condition = newGroupCommand.Condition;
            }
        }

        private static void ValidateAddSharedPersonCommand(ICommand command)
        {
            var addSharedPersonCommand = command as AddSharedPersonToQuestionnaireCommand;
            if (addSharedPersonCommand != null)
            {
                var sharedPersonUserName = Membership.GetUserNameByEmail(addSharedPersonCommand.Email);

                addSharedPersonCommand.PersonId = Membership.GetUser(sharedPersonUserName).ProviderUserKey.AsGuid();
            }
        }

        private static void ValidateRemoveSharedPersonCommand(ICommand command)
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