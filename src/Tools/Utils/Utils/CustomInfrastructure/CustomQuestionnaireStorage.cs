using System;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace Utils.CustomInfrastructure
{
    public class CustomQuestionnaireStorage : IQuestionnaireStorage
    {
        private IQuestionnaire questionnaire = null;
        private QuestionnaireDocument questionnaireDocument = null;
        private readonly IExpressionsPlayOrderProvider expressionsPlayOrderProvider;

        public CustomQuestionnaireStorage(IExpressionsPlayOrderProvider expressionsPlayOrderProvider)
        {
            this.expressionsPlayOrderProvider = expressionsPlayOrderProvider;
        }

        public IQuestionnaire GetQuestionnaire(QuestionnaireIdentity identity, string language)
        {
            return questionnaire;
        }

        public void StoreQuestionnaire(Guid id, long version, QuestionnaireDocument questionnaireDocument)
        {
            this.questionnaireDocument = questionnaireDocument;
            var readOnlyQuestionnaireDocument = questionnaireDocument.AsReadOnly();
            this.questionnaireDocument.ExpressionsPlayOrder = this.expressionsPlayOrderProvider.GetExpressionsPlayOrder(readOnlyQuestionnaireDocument);
            this.questionnaireDocument.DependencyGraph = this.expressionsPlayOrderProvider.GetDependencyGraph(readOnlyQuestionnaireDocument);
            this.questionnaireDocument.ValidationDependencyGraph = this.expressionsPlayOrderProvider.GetValidationDependencyGraph(readOnlyQuestionnaireDocument);

            this.questionnaire = new PlainQuestionnaire(questionnaireDocument, version, null);
        }

        public QuestionnaireDocument GetQuestionnaireDocument(QuestionnaireIdentity identity)
        {
            return this.questionnaireDocument;
        }

        public QuestionnaireDocument GetQuestionnaireDocument(Guid id, long version)
        {
            return this.questionnaireDocument;
        }

        public void DeleteQuestionnaireDocument(Guid id, long version)
        {
            this.questionnaire = null;
            this.questionnaireDocument = null;
        }
    }
}