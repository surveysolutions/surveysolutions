using Main.Core.Documents;
using System;
using System.Linq;
using System.Web.Security;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Designer.Extensions;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Code.Implementation
{
    public class CommandInflater : ICommandInflater
    {
        private readonly IMembershipUserService userHelper;
        private readonly IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader;
        private readonly IReadSideKeyValueStorage<QuestionnaireSharedPersons> sharedPersons;

        public CommandInflater(IMembershipUserService userHelper,
            IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader,
            IReadSideKeyValueStorage<QuestionnaireSharedPersons> sharedPersons)
        {
            this.userHelper = userHelper;
            this.questionnaireDocumentReader = questionnaireDocumentReader;
            this.sharedPersons = sharedPersons;
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
            var currentPasteItemAfterCommand = command as PasteAfter;
            if (currentPasteItemAfterCommand != null)
            {
                currentPasteItemAfterCommand.SourceDocument =
                    GetQuestionnaire(currentPasteItemAfterCommand.SourceQuestionnaireId);
            }

            var currentPasteItemIntoCommand = command as PasteInto;
            if (currentPasteItemIntoCommand != null)
            {
                currentPasteItemIntoCommand.SourceDocument =
                    GetQuestionnaire(currentPasteItemIntoCommand.SourceQuestionnaireId);
            }
        }

        private QuestionnaireDocument GetQuestionnaire(Guid id)
        {
            var questionnaire = this.questionnaireDocumentReader.GetById(id);

            if (questionnaire == null)
            {
                throw new CommandInflaitingException(CommandInflatingExceptionType.EntityNotFound, "Source questionnaire was not found and might be deleted.");
            }

            if (questionnaire.IsPublic || questionnaire.CreatedBy == this.userHelper.WebUser.UserId || this.userHelper.WebUser.IsAdmin)
                return questionnaire;

            var sharedPersons = this.sharedPersons.GetById(id);
            if (sharedPersons == null || sharedPersons.SharedPersons.All(x => x.Id != this.userHelper.WebUser.UserId))
            {
                throw new CommandInflaitingException(CommandInflatingExceptionType.Forbidden, "You don't have permissions to access the source questionnaire");
            }

            return questionnaire;
        }

        private void SetResponsible(ICommand command)
        {
            var currentCommand = command as QuestionnaireCommandBase;
            if (currentCommand == null) return;

            currentCommand.ResponsibleId = this.userHelper.WebUser.UserId;
            currentCommand.IsResponsibleAdmin = this.userHelper.WebUser.IsAdmin;
        }

        private static void ValidateAddSharedPersonCommand(ICommand command)
        {
            var addSharedPersonCommand = command as AddSharedPersonToQuestionnaire;
            if (addSharedPersonCommand != null)
            {
                var sharedPersonUserName = Membership.GetUserNameByEmail(addSharedPersonCommand.Email);

                addSharedPersonCommand.PersonId = Membership.GetUser(sharedPersonUserName).ProviderUserKey.AsGuid();
            }
        }

        private static void ValidateRemoveSharedPersonCommand(ICommand command)
        {
            var removeSharedPersonCommand = command as RemoveSharedPersonFromQuestionnaire;
            if (removeSharedPersonCommand != null)
            {
                var sharedPersonUserName = Membership.GetUserNameByEmail(removeSharedPersonCommand.Email);
                removeSharedPersonCommand.PersonId = Membership.GetUser(sharedPersonUserName).ProviderUserKey.AsGuid();
            }
        }
    }
}