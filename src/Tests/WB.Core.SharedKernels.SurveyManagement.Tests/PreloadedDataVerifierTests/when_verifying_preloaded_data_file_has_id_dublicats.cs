﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_file_has_id_dublicats : PreloadedDataVerifierTestContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            questionnaire = CreateQuestionnaireDocumentWithOneChapter();
            questionnaire.Title = "questionnaire";
            preloadedDataByFile = CreatePreloadedDataByFile(new[] { "Id", "ParentId" }, new string[][] { new string[] { "1", "1" }, new string[] { "1", "1" } },
                "questionnaire.csv");
            preloadedDataVerifier = CreatePreloadedDataVerifier(questionnaire);
        };

        Because of =
            () =>
                result =
                    preloadedDataVerifier.Verify(questionnaireId, 1, new[] { preloadedDataByFile });

        It should_result_has_1_error = () =>
            result.Count().ShouldEqual(1);

        It should_return_single_PL0006_error = () =>
            result.First().Code.ShouldEqual("PL0006");

        It should_return_reference_with_Cell_type = () =>
            result.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Cell);

        It should_error_PositionX_be_equal_to_0 = () =>
          result.First().References.First().PositionX.ShouldEqual(0);

        It should_error_PositionY_be_equal_to_1 = () =>
          result.First().References.First().PositionY.ShouldEqual(1);

        It should_error_has_content_with_id_and_parent_id = () =>
            result.First().References.First().Content.ShouldEqual("id:1, parentId: 1");

        private static PreloadedDataVerifier preloadedDataVerifier;
        private static IEnumerable<PreloadedDataVerificationError> result;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static PreloadedDataByFile preloadedDataByFile;
    }
}
