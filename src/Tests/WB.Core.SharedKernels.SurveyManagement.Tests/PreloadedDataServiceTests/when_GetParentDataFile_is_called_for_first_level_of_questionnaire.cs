using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.PreloadedDataServiceTests
{
    internal class when_GetParentDataFile_is_called_for_first_level_of_questionnaire : PreloadedDataServiceTestContext
    {
        Establish context = () =>
        {
            questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(new Group("Roster Group")
                {
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    RosterFixedTitles = new[] { "1" },
                    VariableName = "roster",
                    PublicKey = rosterGroupId
                });

            preloadedDataService = CreatePreloadedDataService(questionnaireDocument);
        };

        Because of =
           () =>
               result =
                   preloadedDataService.GetParentDataFile("Roster Group", new[] { CreatePreloadedDataByFile(null, null, "Roster Group"), CreatePreloadedDataByFile(null, null, questionnaireDocument.Title) });

        It should_return_not_null_result = () =>
            result.ShouldNotBeNull();

        It should_result_filename_be_equal_to_top_level_file = () =>
            result.FileName.SequenceEqual(questionnaireDocument.Title);

        private static PreloadedDataService preloadedDataService;
        private static QuestionnaireDocument questionnaireDocument;
        private static PreloadedDataByFile result;
        private static Guid rosterGroupId = Guid.NewGuid();
    }
}
