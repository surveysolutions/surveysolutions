using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_file_has_orphan_roster_data : PreloadedDataVerifierTestContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            questionnaire =
                CreateQuestionnaireDocumentWithOneChapter(new Group(rosterTitle)
                {
                    IsRoster = true,
                    PublicKey = Guid.NewGuid(),
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    RosterFixedTitles = new[] { "a" }
                });
            questionnaire.Title = questionnaireTitle;
            preloadedDataByFileTopLevel = CreatePreloadedDataByFile(new[] { "Id", "ParentId" }, new string[0][],
                questionnaireTitle + ".csv");
            preloadedDataByFileRosterLevel = CreatePreloadedDataByFile(new[] { "Id", "ParentId" }, new string[][] { new string[] { "0", "1" } },
                rosterTitle + ".csv");
            preloadedDataVerifier = CreatePreloadedDataVerifier(questionnaire);
        };

        Because of =
            () =>
                result =
                    preloadedDataVerifier.Verify(questionnaireId, 1, new[] { preloadedDataByFileTopLevel, preloadedDataByFileRosterLevel });

        It should_result_has_1_error = () =>
            result.Count().ShouldEqual(1);

        It should_error_has_code_PL0008 = () =>
            result.First().Code.ShouldEqual("PL0008");

        It should_error_has_type_of_reference_cell = () =>
            result.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Cell);

        It should_error_PositionX_be_equal_to_1 = () =>
          result.First().References.First().PositionX.ShouldEqual(1);

        It should_error_PositionY_be_equal_to_0 = () =>
          result.First().References.First().PositionY.ShouldEqual(0);

        It should_error_has_content_id_of_orphan_record = () =>
            result.First().References.First().Content.ShouldEqual("1");

        private static PreloadedDataVerifier preloadedDataVerifier;
        private static IEnumerable<PreloadedDataVerificationError> result;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static PreloadedDataByFile preloadedDataByFileTopLevel;
        private static PreloadedDataByFile preloadedDataByFileRosterLevel;
        private static string questionnaireTitle = "questionnaire";
        private static string rosterTitle = "roster";
    }
}
