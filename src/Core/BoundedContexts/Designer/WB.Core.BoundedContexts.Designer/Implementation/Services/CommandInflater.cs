using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Microsoft.EntityFrameworkCore;
using WB.Core.BoundedContexts.Designer.Classifications;
using WB.Core.BoundedContexts.Designer.Commands;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Designer.Code.Implementation;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class CommandInflater : ICommandInflater
    {
        private readonly IDesignerQuestionnaireStorage questionnaireStorage;
        private readonly DesignerDbContext dbContext;
        private readonly IClassificationsStorage classificationsStorage;
        private readonly ILoggedInUser user;

        public CommandInflater(
            IDesignerQuestionnaireStorage questionnaireStorage,
            DesignerDbContext dbContext,
            IClassificationsStorage classificationsStorage,
            ILoggedInUser user)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.dbContext = dbContext;
            this.classificationsStorage = classificationsStorage;
            this.user = user ?? throw new ArgumentNullException(nameof(user));
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
                    GetQuestionnaire(currentPasteItemAfterCommand.SourceQuestionnaireRevision ??
                        new QuestionnaireRevision(currentPasteItemAfterCommand.SourceQuestionnaireId));
            }

            if (command is PasteInto currentPasteItemIntoCommand)
            {
                currentPasteItemIntoCommand.SourceDocument =
                    GetQuestionnaire(currentPasteItemIntoCommand.SourceQuestionnaireRevision ??
                        new QuestionnaireRevision(currentPasteItemIntoCommand.SourceQuestionnaireId));
            }
        }

        private QuestionnaireDocument GetQuestionnaire(QuestionnaireRevision questionnaireRevision)
        {
            var questionnaire = this.questionnaireStorage.Get(questionnaireRevision);

            if (questionnaire == null)
            {
                throw new CommandInflaitingException(CommandInflatingExceptionType.EntityNotFound,
                    "Source questionnaire was not found and might be deleted.");
            }

            // When a specific revision is requested, authorization must be evaluated against the
            // current (base) questionnaire so that visibility/ownership changes that occurred after
            // the snapshot was taken are honoured rather than bypassed.
            var currentQuestionnaire = questionnaireRevision.Revision.HasValue
                ? this.questionnaireStorage.Get(questionnaireRevision.QuestionnaireId)
                  ?? questionnaire
                : questionnaire;

            if (currentQuestionnaire.IsPublic || currentQuestionnaire.CreatedBy == this.user.Id ||
                this.user.IsAdmin)
                return questionnaire;

            var questionnaireListItem = this.dbContext.Questionnaires.Include(x => x.SharedPersons)
                .FirstOrDefault(x => x.QuestionnaireId == questionnaireRevision.QuestionnaireId.FormatGuid());
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

            var sharedWith = this.dbContext.Users.FindByNameOrEmail(addSharedPersonCommand.EmailOrLogin);

            if (sharedWith == null)
            {
                throw new CommandInflaitingException(CommandInflatingExceptionType.Common,
                    "User was not found");
            }
            
            if (sharedWith.Email == null)
            {
                throw new CommandInflaitingException(CommandInflatingExceptionType.Common,
                    "Invalid user");
            }

            addSharedPersonCommand.PersonId = sharedWith.Id;
            addSharedPersonCommand.EmailOrLogin = sharedWith.Email;
           
        }

        private void ValidateRemoveSharedPersonCommand(ICommand command)
        {
            if (!(command is RemoveSharedPersonFromQuestionnaire removeSharedPersonCommand)) 
                return;

            var unshareWithById = this.dbContext.Users.Find(removeSharedPersonCommand.PersonId);
            if (unshareWithById != null)
            {
                removeSharedPersonCommand.PersonId = unshareWithById.Id;
                removeSharedPersonCommand.Email = unshareWithById.Email;
            }
        }
    }
}
