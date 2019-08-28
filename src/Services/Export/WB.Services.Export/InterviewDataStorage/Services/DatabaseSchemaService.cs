using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;

namespace WB.Services.Export.InterviewDataStorage.Services
{
    public class DatabaseSchemaService : IDatabaseSchemaService
    {
        private readonly IQuestionnaireSchemaGenerator questionnaireSchemaGenerator;
        private readonly TenantDbContext dbContext;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public DatabaseSchemaService(
            IQuestionnaireSchemaGenerator questionnaireSchemaGenerator,
            TenantDbContext dbContext,
            IQuestionnaireStorage questionnaireStorage)
        {
            this.questionnaireSchemaGenerator = questionnaireSchemaGenerator;
            this.dbContext = dbContext;
            this.questionnaireStorage = questionnaireStorage;
        }

        public async Task CreateQuestionnaireDbStructureAsync(QuestionnaireId questionnaireId, CancellationToken cancellationToken)
        {
            var questionnaireDocument = await questionnaireStorage.GetQuestionnaireAsync(questionnaireId, cancellationToken);
            CreateQuestionnaireDbStructure(questionnaireDocument);
        }

        public void CreateQuestionnaireDbStructure(QuestionnaireDocument questionnaireDocument)
        {
            if (this.dbContext.Database.IsNpgsql())
            {
                this.dbContext.Database.Migrate();
            }
            var reference = this.dbContext.GeneratedQuestionnaires.Find(questionnaireDocument.QuestionnaireId.ToString());

            if (reference != null)
                return;

            reference = new GeneratedQuestionnaireReference(questionnaireDocument.QuestionnaireId);
            this.dbContext.GeneratedQuestionnaires.Add(reference);

            questionnaireSchemaGenerator.CreateQuestionnaireDbStructure(questionnaireDocument);
        }

        public bool TryDropQuestionnaireDbStructure(QuestionnaireDocument questionnaireDocument)
        {
            var reference = this.dbContext.GeneratedQuestionnaires.Find(questionnaireDocument.QuestionnaireId.ToString());

            if (reference == null)
            {
                reference = new GeneratedQuestionnaireReference(questionnaireDocument.QuestionnaireId);
                this.dbContext.GeneratedQuestionnaires.Add(reference);
            }

            if (reference.DeletedAt == null)
            {
                this.questionnaireSchemaGenerator.DropQuestionnaireDbStructure(questionnaireDocument);

                reference.DeletedAt = DateTime.UtcNow;
                this.dbContext.SaveChanges();
                return true;
            }

            return false;
        }
    }
}
