using Main.Core.Documents;
using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Identity;
using WB.Core.BoundedContexts.Designer.Classifications;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.Designer.Code.Implementation
{
    public class CommandInflater : ICommandInflater
    {
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader;
        private readonly IPlainStorageAccessor<QuestionnaireListViewItem> questionnairesList;
        private readonly IClassificationsStorage classificationsStorage;
        private readonly ILoggedInUser user;
        private readonly IIdentityService identityService;

        public CommandInflater(
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader,
            IPlainStorageAccessor<QuestionnaireListViewItem> questionnairesList,
            IClassificationsStorage classificationsStorage,
            ILoggedInUser user,
            IIdentityService identityService)
        {
            this.questionnaireDocumentReader = questionnaireDocumentReader;
            this.questionnairesList = questionnairesList;
            this.classificationsStorage = classificationsStorage;
            this.user = user ?? throw new ArgumentNullException(nameof(user));
            this.identityService = identityService;
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

            if (questionnaire.IsPublic || questionnaire.CreatedBy == this.user.Id ||
                this.user.IsAdmin)
                return questionnaire;

            var questionnaireListItem = this.questionnairesList.GetById(id.FormatGuid());
            if (questionnaireListItem == null ||
                questionnaireListItem.SharedPersons.All(x => x.UserId != this.user.Id))
            {
                throw new CommandInflaitingException(CommandInflatingExceptionType.Forbidden,
                    "You don't have permissions to access the source questionnaire");
            }

            return questionnaire;
        }

        private void SetResponsible(ICommand command)
        {
            if (!(command is QuestionnaireCommandBase currentCommand)) return;

            currentCommand.ResponsibleId = this.user.Id;
            currentCommand.IsResponsibleAdmin = this.user.IsAdmin;
        }

        private void ValidateAddSharedPersonCommand(ICommand command)
        {
            if (!(command is AddSharedPersonToQuestionnaire addSharedPersonCommand)) 
                return;

            var user = identityService.GetByNameOrEmail(addSharedPersonCommand.EmailOrLogin);

            addSharedPersonCommand.PersonId = user.Id;
            addSharedPersonCommand.EmailOrLogin = user.Email;
        }

        private void ValidateRemoveSharedPersonCommand(ICommand command)
        {
            if (!(command is RemoveSharedPersonFromQuestionnaire removeSharedPersonCommand)) 
                return;

            var user = identityService.GetByNameOrEmail(removeSharedPersonCommand.Email);
            removeSharedPersonCommand.PersonId = user.Id;
            removeSharedPersonCommand.Email = user.Email;
        }
    }
}
