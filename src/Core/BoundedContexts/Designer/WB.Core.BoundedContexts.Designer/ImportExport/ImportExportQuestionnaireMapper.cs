using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ImportExport.Models;

namespace WB.Core.BoundedContexts.Designer.ImportExport
{
    public class ImportExportQuestionnaireMapper : IImportExportQuestionnaireMapper
    {
        private readonly QuestionnaireDocumentMapper documentMapper;

        public ImportExportQuestionnaireMapper()
        {
            this.documentMapper = new QuestionnaireDocumentMapper();
        }

        public Questionnaire Map(QuestionnaireDocument questionnaireDocument)
        {
            return documentMapper.Map(questionnaireDocument);
        }

        public QuestionnaireDocument Map(Questionnaire questionnaire)
        {
            return documentMapper.Map(questionnaire);
        }
    }
}