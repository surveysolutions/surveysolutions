using Main.Core.Documents;
using System;
using System.Linq;
using System.Web.Security;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.Services.Accounts;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Designer.Extensions;

namespace WB.UI.Designer.Code.Implementation
{
    public class CommandInflater : ICommandInflater
    {
        private readonly IMembershipUserService userHelper;
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader;
        private readonly IPlainStorageAccessor<QuestionnaireListViewItem> questionnairesList;
        private readonly IAccountRepository accountRepository;

        public CommandInflater(IMembershipUserService userHelper,
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader,
            IPlainStorageAccessor<QuestionnaireListViewItem> questionnairesList,
            IAccountRepository accountRepository)
        {
            this.userHelper = userHelper;
            this.questionnaireDocumentReader = questionnaireDocumentReader;
            this.questionnairesList = questionnairesList;
            this.accountRepository = accountRepository;
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
            var questionnaire = this.questionnaireDocumentReader.GetById(id.FormatGuid());

            if (questionnaire == null)
            {
                throw new CommandInflaitingException(CommandInflatingExceptionType.EntityNotFound,
                    "Source questionnaire was not found and might be deleted.");
            }

            if (questionnaire.IsPublic || questionnaire.CreatedBy == this.userHelper.WebUser.UserId ||
                this.userHelper.WebUser.IsAdmin)
                return questionnaire;

            var questionnaireListItem = this.questionnairesList.GetById(id.FormatGuid());
            if (questionnaireListItem == null ||
                questionnaireListItem.SharedPersons.All(x => x.UserId != this.userHelper.WebUser.UserId))
            {
                throw new CommandInflaitingException(CommandInflatingExceptionType.Forbidden,
                    "You don't have permissions to access the source questionnaire");
            }

            return questionnaire;
        }

        private void SetResponsible(ICommand command)
        {
            if (!(command is QuestionnaireCommandBase currentCommand)) return;

            currentCommand.ResponsibleId = this.userHelper.WebUser.UserId;
            currentCommand.IsResponsibleAdmin = this.userHelper.WebUser.IsAdmin;
        }

        private void ValidateAddSharedPersonCommand(ICommand command)
        {
            if (!(command is AddSharedPersonToQuestionnaire addSharedPersonCommand)) 
                return;

            var user = accountRepository.GetByNameOrEmail(addSharedPersonCommand.EmailOrLogin);

            addSharedPersonCommand.PersonId = user.ProviderUserKey;
            addSharedPersonCommand.EmailOrLogin = user.Email;
        }

        private void ValidateRemoveSharedPersonCommand(ICommand command)
        {
            if (!(command is RemoveSharedPersonFromQuestionnaire removeSharedPersonCommand)) 
                return;

            var user = accountRepository.GetByNameOrEmail(removeSharedPersonCommand.Email);
            removeSharedPersonCommand.PersonId = user.ProviderUserKey;
            removeSharedPersonCommand.Email = user.Email;
        }
    }
}
