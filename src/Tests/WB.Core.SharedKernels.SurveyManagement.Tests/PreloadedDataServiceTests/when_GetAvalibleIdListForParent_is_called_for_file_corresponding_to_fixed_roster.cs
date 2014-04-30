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
    internal class when_GetAvalibleIdListForParent_is_called_for_file_corresponding_to_fixed_roster : PreloadedDataServiceTestContext
    {
        Establish context = () =>
        {
            questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(new Group("Roster Group")
                {
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    RosterFixedTitles = new[] { "1","2" },
                    PublicKey = rosterGroupId
                });

            preloadedDataService = CreatePreloadedDataService(questionnaireDocument);
        };

        Because of =
            () =>
                result =
                    preloadedDataService.GetAvalibleIdListForParent(
                        CreatePreloadedDataByFile(new string[] { "Id" }, new string[][] { new string[] { "1" } },
                            questionnaireDocument.Title), rosterGroupId, "1");

        It should_result_be_not_null = () =>
            result.ShouldNotBeNull();

        It should_result_have_2_ids_1_and_2 = () =>
            result.SequenceEqual(new decimal[] { 1, 2 });

        private static PreloadedDataService preloadedDataService;
        private static QuestionnaireDocument questionnaireDocument;
        private static decimal[] result;
        private static Guid rosterGroupId = Guid.NewGuid();
    }
}
