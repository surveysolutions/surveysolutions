using System;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider
{
    public class DesignerQuestionnaireStorage : IDesignerQuestionnaireStorage
    {
        private readonly IQuestionnaireHistoryVersionsService questionnaireHistoryVersionsService;
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader;

        public DesignerQuestionnaireStorage(IQuestionnaireHistoryVersionsService questionnaireHistoryVersionsService,
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader)
        {
            this.questionnaireHistoryVersionsService = questionnaireHistoryVersionsService;
            this.questionnaireDocumentReader = questionnaireDocumentReader;
        }

        public QuestionnaireDocument? Get(QuestionnaireRevision questionnaire)
        {
            return questionnaire.Revision == null
                ? this.questionnaireDocumentReader.GetById((questionnaire.OriginalQuestionnaireId ?? questionnaire.QuestionnaireId).FormatGuid())
                : this.questionnaireHistoryVersionsService.GetByHistoryVersion(questionnaire.Revision.Value);
        }

        public QuestionnaireDocument? Get(Guid questionnaireId)
        {
            return this.questionnaireDocumentReader.GetById(questionnaireId.FormatGuid());
        }
    }
}
