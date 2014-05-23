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
    internal class when_PreparePreloadedData_is_called_for_array_from_one_element : PreloadedDataServiceTestContext
    {
        Establish context = () =>
        {
            questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(new Group("Roster Group")
                {
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    RosterFixedTitles = new[] { "1" },
                    PublicKey = rosterGroupId
                });

            preloadedDataService = CreatePreloadedDataService(questionnaireDocument);
        };

        Because of =
           () =>
               result =
                   preloadedDataService.PreparePreloadedData(new[] { CreatePreloadedDataByFile(null, null, "random file name")});

        It should_return_result_form_one_element = () =>
            result.Length.ShouldEqual(1);

        It should_first_element_of_result_filename_be_equal_to_questionnaire_title = () =>
            result[0].FileName.ShouldEqual(questionnaireDocument.Title);

        private static PreloadedDataService preloadedDataService;
        private static QuestionnaireDocument questionnaireDocument;
        private static PreloadedDataByFile[] result;
        private static Guid rosterGroupId = Guid.NewGuid();
    }
}
