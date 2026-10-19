using System;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.AnonymousQuestionnaires;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.DataAccess;
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

        public AnonymousSharingPostProcessor(DesignerDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public void Process(Questionnaire aggregate, EnableAnonymousSharing command)
        {
            var anonymousQuestionnaire = this.Find(command.QuestionnaireId);
            if (anonymousQuestionnaire == null)
                this.Create(command.QuestionnaireId);
            else
                anonymousQuestionnaire.IsActive = true;

            this.dbContext.SaveChanges();
        }

        public void Process(Questionnaire aggregate, DisableAnonymousSharing command)
        {
            var anonymousQuestionnaire = this.Find(command.QuestionnaireId);
            if (anonymousQuestionnaire == null)
                return;

            anonymousQuestionnaire.IsActive = false;
            this.dbContext.SaveChanges();
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
    }
}

