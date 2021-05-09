using System;
using System.Collections.Generic;
using Moq;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Tests.CsvExport.Implementation.DoFiles
{
    internal class QuestionsExportDoFilesContext : StataEnvironmentContentGeneratorTestContext
    {
        public IInterviewsDoFilesExporter CreateInterviewsDoFilesExporter(Action<string> returnContentAction)
        {
            var fileSystemAccessor = CreateFileSystemAccessor(returnContentAction);
            var exporter = Create.InterviewsDoFilesExporter(fileSystemAccessor);
            return exporter;
        }

        public QuestionnaireExportStructure CreateQuestionnaireExportStructure(List<Categories> reusableCategories, IQuestionnaireEntity[] children)
        {
            var questionnaireDocument = Create.QuestionnaireDocument(children: children);
            questionnaireDocument.Categories = reusableCategories;
            questionnaireDocument.VariableName = "questionnaire_var";

            var exportStructureFactory = new QuestionnaireExportStructureFactory(Mock.Of<IQuestionnaireStorage>());
            var exportStructure = exportStructureFactory.CreateQuestionnaireExportStructure(questionnaireDocument);
            return exportStructure;
        }

        public QuestionnaireExportStructure CreateQuestionnaireExportStructure(IQuestionnaireEntity[] children)
        {
            return CreateQuestionnaireExportStructure(null, children);
        }
    }
}
