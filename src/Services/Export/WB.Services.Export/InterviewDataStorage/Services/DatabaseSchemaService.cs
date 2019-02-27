using System;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.InterviewDataStorage.Services
{
    public class DatabaseSchemaService : IDatabaseSchemaService
    {
        private readonly IQuestionnaireSchemaGenerator questionnaireSchemaGenerator;
        private readonly TenantDbContext dbContext;

        public DatabaseSchemaService(
            IQuestionnaireSchemaGenerator questionnaireSchemaGenerator,
            TenantDbContext dbContext)
        {
            this.questionnaireSchemaGenerator = questionnaireSchemaGenerator;
            this.dbContext = dbContext;
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
                return true;
            }

            return false;
        }
    }
}
