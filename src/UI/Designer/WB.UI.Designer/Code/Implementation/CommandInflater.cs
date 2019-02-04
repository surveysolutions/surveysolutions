using Main.Core.Documents;
using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Classifications;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.Services.Accounts;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.Designer.Code.Implementation
{
    public class CommandInflater : ICommandInflater
    {
        private readonly IMembershipUserService userHelper;
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader;
        private readonly IPlainStorageAccessor<QuestionnaireListViewItem> questionnairesList;
        private readonly IAccountRepository accountRepository;
        private readonly IClassificationsStorage classificationsStorage;

        public CommandInflater(IMembershipUserService userHelper,
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader,
            IPlainStorageAccessor<QuestionnaireListViewItem> questionnairesList,
            IAccountRepository accountRepository, 
            IClassificationsStorage classificationsStorage)
        {
            this.userHelper = userHelper;
            this.questionnaireDocumentReader = questionnaireDocumentReader;
            this.questionnairesList = questionnairesList;
            this.accountRepository = accountRepository;
            this.classificationsStorage = classificationsStorage;
        }

        public void PrepareDeserializedCommandForExecution(ICommand command)
        {
            this.SetResponsible(command);
            ValidateAddSharedPersonCommand(command);
            ValidateRemoveSharedPersonCommand(command);

            this.InflateCopyPasteProperties(command);
            this.InflateCategoriesFromClassification(command);
        }

        private void InflateCategoriesFromClassification(ICommand command)
        {
            if (!(command is ReplaceOptionsWithClassification classificationCommand)) return;

            var options = classificationsStorage.GetCategories(classificationCommand.ClassificationId)
                .Result
                .Select(x => new Option(x.Value.ToString(), x.Title))
                .ToArray();
                    
            classificationCommand.Options = options;
        }

        private void InflateCopyPasteProperties(ICommand command)
        {
            if (command is PasteAfter currentPasteItemAfterCommand)
            {
                currentPasteItemAfterCommand.SourceDocument =
                    GetQuestionnaire(currentPasteItemAfterCommand.SourceQuestionnaireId);
            }

            if (command is PasteInto currentPasteItemIntoCommand)
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
