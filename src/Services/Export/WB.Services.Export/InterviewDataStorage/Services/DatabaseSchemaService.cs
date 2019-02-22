using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;

namespace WB.Services.Export.InterviewDataStorage.Services
{
    public class DatabaseSchemaService : IDatabaseSchemaService
    {
        private readonly IQuestionnaireSchemaGenerator questionnaireSchemaGenerator;
        private readonly TenantDbContext dbContext;
        private readonly IQuestionnaireStorageCache cache;

        public DatabaseSchemaService(
            IQuestionnaireSchemaGenerator questionnaireSchemaGenerator,
            TenantDbContext dbContext,
            IQuestionnaireStorageCache cache)
        {
            this.questionnaireSchemaGenerator = questionnaireSchemaGenerator;
            this.dbContext = dbContext;
            this.cache = cache;
        }

        public void CreateQuestionnaireDbStructure(QuestionnaireDocument questionnaireDocument)
        {
            var reference = this.dbContext.GeneratedQuestionnaires.Find(questionnaireDocument.QuestionnaireId.ToString());

            if (reference != null)
                return;

            reference = new GeneratedQuestionnaireReference(questionnaireDocument.QuestionnaireId);
            this.dbContext.GeneratedQuestionnaires.Add(reference);

            questionnaireSchemaGenerator.CreateQuestionnaireDbStructure(questionnaireDocument);
        }

        public void DropQuestionnaireDbStructure(QuestionnaireDocument questionnaireDocument)
        {
            var reference = this.dbContext.GeneratedQuestionnaires.Find(questionnaireDocument.QuestionnaireId.ToString());

            if (reference == null)
            {
                reference = new GeneratedQuestionnaireReference(questionnaireDocument.QuestionnaireId);
                this.dbContext.GeneratedQuestionnaires.Add(reference);
            }

            if (reference.DeletedAt == null)
            {
                this.cache.Remove(questionnaireDocument.QuestionnaireId);

                this.questionnaireSchemaGenerator.DropQuestionnaireDbStructure(questionnaireDocument);

                reference.DeletedAt = DateTime.UtcNow;
            }
        }
    }
}
