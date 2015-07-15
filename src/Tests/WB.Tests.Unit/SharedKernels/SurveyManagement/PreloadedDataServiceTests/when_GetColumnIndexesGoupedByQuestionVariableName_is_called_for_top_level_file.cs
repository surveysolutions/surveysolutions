using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataServiceTests
{
    internal class when_GetColumnIndexesGoupedByQuestionVariableName_is_called_for_top_level_file : PreloadedDataServiceTestContext
    {
        Establish context = () =>
        {
            questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(
                    new NumericQuestion() { StataExportCaption = "nq1", QuestionType = QuestionType.Numeric, PublicKey = Guid.NewGuid()},
                    new TextQuestion() { StataExportCaption = "tq1", QuestionType = QuestionType.Text, PublicKey = Guid.NewGuid() });

            preloadedDataService = CreatePreloadedDataService(questionnaireDocument);
        };

        Because of =
           () =>
               result =
                   preloadedDataService.GetColumnIndexesGoupedByQuestionVariableName(CreatePreloadedDataByFile(new string[]{"nq1"}, null, questionnaireDocument.Title) );

        It should_return_not_null_result = () =>
           result.ShouldNotBeNull();

        It should_result_has_index_of_numeric_question_only = () =>
          result["nq1"].Select(r=>r.Item2).ToArray().SequenceEqual(new []{0});

        private static PreloadedDataService preloadedDataService;
        private static QuestionnaireDocument questionnaireDocument;
        private static Dictionary<string, Tuple<string, int>[]> result;
        private static Guid rosterGroupId = Guid.NewGuid();
    }
}
