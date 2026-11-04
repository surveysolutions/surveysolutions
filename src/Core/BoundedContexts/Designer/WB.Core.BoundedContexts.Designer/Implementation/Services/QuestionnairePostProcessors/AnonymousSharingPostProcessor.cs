using System;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.AnonymousQuestionnaires;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.Infrastructure.CommandBus;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.QuestionnairePostProcessors
{
    // Anonymous sharing state is stored outside of the questionnaire document,
    // so it is persisted here once the aggregate has accepted the command.
    internal class AnonymousSharingPostProcessor :
        ICommandPostProcessor<Questionnaire, EnableAnonymousSharing>,
        ICommandPostProcessor<Questionnaire, DisableAnonymousSharing>,
        ICommandPostProcessor<Questionnaire, RegenerateAnonymousSharingLink>
    {
        private readonly DesignerDbContext dbContext;
        private readonly IQuestionnaireHistoryVersionsService historyVersionsService;

        public AnonymousSharingPostProcessor(
            DesignerDbContext dbContext,
            IQuestionnaireHistoryVersionsService historyVersionsService)
        {
            this.dbContext = dbContext;
            this.historyVersionsService = historyVersionsService;
        }

        public void Process(Questionnaire aggregate, EnableAnonymousSharing command)
        {
            var anonymousQuestionnaire = this.Find(command.QuestionnaireId);

            if (anonymousQuestionnaire?.IsActive == true)
                return;

            if (anonymousQuestionnaire == null)
                this.Create(command.QuestionnaireId);
            else
                anonymousQuestionnaire.IsActive = true;

            this.AddHistoryItem(aggregate, command, QuestionnaireActionType.AnonymousSharingEnabled);
        }

        public void Process(Questionnaire aggregate, DisableAnonymousSharing command)
        {
            var anonymousQuestionnaire = this.Find(command.QuestionnaireId);

            if (anonymousQuestionnaire?.IsActive != true)
                return;

            anonymousQuestionnaire.IsActive = false;
            this.AddHistoryItem(aggregate, command, QuestionnaireActionType.AnonymousSharingDisabled);
        }

        public void Process(Questionnaire aggregate, RegenerateAnonymousSharingLink command)
        {
            var anonymousQuestionnaire = this.Find(command.QuestionnaireId);
            if (anonymousQuestionnaire != null)
                this.dbContext.AnonymousQuestionnaires.Remove(anonymousQuestionnaire);

            this.Create(command.QuestionnaireId);
            this.dbContext.SaveChanges();
        }

        private AnonymousQuestionnaire? Find(Guid questionnaireId)
            => this.dbContext.AnonymousQuestionnaires.FirstOrDefault(a => a.QuestionnaireId == questionnaireId);

        private void Create(Guid questionnaireId)
            => this.dbContext.AnonymousQuestionnaires.Add(new AnonymousQuestionnaire
            {
                QuestionnaireId = questionnaireId,
                AnonymousQuestionnaireId = Guid.NewGuid(),
                IsActive = true,
                GeneratedAtUtc = DateTime.UtcNow
            });

        private void AddHistoryItem(
            Questionnaire aggregate,
            QuestionnaireCommand command,
            QuestionnaireActionType actionType)
        {
            var userName = this.dbContext.Users.Find(command.ResponsibleId)?.UserName;
            this.historyVersionsService.AddQuestionnaireChangeItem(
                command.QuestionnaireId,
                command.ResponsibleId,
                userName,
                actionType,
                QuestionnaireItemType.Questionnaire,
                command.QuestionnaireId,
                aggregate.QuestionnaireDocument.Title,
                null,
                null,
                null,
                null);
        }
    }
}
