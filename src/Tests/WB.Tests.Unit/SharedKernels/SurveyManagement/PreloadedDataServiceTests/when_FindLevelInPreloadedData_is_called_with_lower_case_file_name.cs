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
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataServiceTests
{
    internal class when_FindLevelInPreloadedData_is_called_with_lower_case_file_name : PreloadedDataServiceTestContext
    {
        Establish context = () =>
        {
            questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(
                    Create.Entity.FixedRoster(rosterId: rosterGroupId,
                        obsoleteFixedTitles: new[] {"1"}, title: "Roster Group", variable: "roster"));

            preloadedDataService = CreatePreloadedDataService(questionnaireDocument);
        };

        Because of =
           () =>
               result =
                   preloadedDataService.FindLevelInPreloadedData("roster");

        It should_return_not_null_result = () =>
           result.ShouldNotBeNull();

        It should_result_levelId_be_equal_to_rosterGroupId = () =>
          result.LevelScopeVector.SequenceEqual(new[] { rosterGroupId });

        private static PreloadedDataService preloadedDataService;
        private static QuestionnaireDocument questionnaireDocument;
        private static HeaderStructureForLevel result;
        private static Guid rosterGroupId = Guid.NewGuid();
    }
}
