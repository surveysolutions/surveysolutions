using Main.Core.Documents;
using System;
using System.Web.Security;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Designer.Extensions;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Code.Implementation
{
    public class CommandInflater : ICommandInflater
    {
        private readonly IMembershipUserService userHelper;
        private readonly IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory;

        public CommandInflater(IMembershipUserService userHelper, 
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory)
        {
            this.userHelper = userHelper;
            this.questionnaireViewFactory = questionnaireViewFactory;
        }

        public void PrepareDeserializedCommandForExecution(ICommand command)
        {
            this.SetResponsible(command);
            ValidateAddSharedPersonCommand(command);
            ValidateRemoveSharedPersonCommand(command);

            this.InflateCopyPasteProperties(command);
        }

        private void InflateCopyPasteProperties(ICommand command)
        {
            var currentPasteItemAfterCommand = command as PasteItemAfterCommand;
            if (currentPasteItemAfterCommand != null)
            {
                currentPasteItemAfterCommand.SourceDocument =
                    GetQuestionnaire(currentPasteItemAfterCommand.SourceQuestionnaireId);

                //remove after test
                command = new CloneGroupCommand(
                    currentPasteItemAfterCommand.QuestionnaireId,
                    currentPasteItemAfterCommand.EntityId,
                    currentPasteItemAfterCommand.ResponsibleId,
                    currentPasteItemAfterCommand.SourceItemId,
                    1);
            }

            var currentPasteItemIntoCommand = command as PasteItemIntoCommand;
            if (currentPasteItemIntoCommand != null)
            {
                currentPasteItemIntoCommand.SourceDocument =
                    GetQuestionnaire(currentPasteItemIntoCommand.SourceQuestionnaireId);

                //remove after test
                command = new CloneGroupCommand(
                    currentPasteItemIntoCommand.QuestionnaireId,
                    currentPasteItemIntoCommand.EntityId,
                    currentPasteItemIntoCommand.ResponsibleId,
                    currentPasteItemIntoCommand.SourceItemId,
                    1);
            }
        }

        private QuestionnaireDocument GetQuestionnaire(Guid id)
        {
            var questionnaire = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));

            if (questionnaire == null)
            {
                throw new CommandInflaitingException("Source questionnaire was not found and might be deleted.");
            }

            if (!questionnaire.Source.IsPublic && (questionnaire.Source.CreatedBy != this.userHelper.WebUser.UserId && !questionnaire.Source.SharedPersons.Contains(this.userHelper.WebUser.UserId)))
            {
                throw new CommandInflaitingException("You don't have permissions to access the source questionnaire");
            }

            return questionnaire.Source;
        }

        private void SetResponsible(ICommand command)
        {
            var currentCommand = command as QuestionnaireCommandBase;
            if (currentCommand != null)
            {
                currentCommand.ResponsibleId = this.userHelper.WebUser.UserId;
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