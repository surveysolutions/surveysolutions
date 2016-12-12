using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataServiceTests
{
    internal class when_GetParentDataFile_is_called_for_first_level_of_questionnaire : PreloadedDataServiceTestContext
    {
        Establish context = () =>
        {
            questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(Create.Entity.FixedRoster(rosterId: rosterGroupId, title: "Roster Group", variable: "roster",
                        obsoleteFixedTitles: new[] { "1" }));

            preloadedDataService = CreatePreloadedDataService(questionnaireDocument);
        };

        Because of =
           () =>
               result =
                   preloadedDataService.GetParentDataFile("roster", new[] { CreatePreloadedDataByFile(null, null, "roster"), CreatePreloadedDataByFile(null, null, questionnaireDocument.Title) });

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
