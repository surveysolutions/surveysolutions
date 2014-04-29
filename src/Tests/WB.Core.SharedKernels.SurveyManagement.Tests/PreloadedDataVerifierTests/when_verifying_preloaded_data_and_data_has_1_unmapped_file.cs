﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.PreloadedData;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_and_data_has_1_unmapped_file : PreloadedDataVerifierTestContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter();
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            preloadedDataVerifier = CreatePreloadedDataVerifier(questionnaire);
        };

        Because of =
            () =>
                result =
                    preloadedDataVerifier.Verify(questionnaireId, 1, new[] { CreatePreloadedDataByFile() });
        
        It should_result_has_1_error = () =>
           result.Count().ShouldEqual(1);

        It should_return_single_PL0004_error = () =>
            result.First().Code.ShouldEqual("PL0004");

        It should_return_reference_with_File_type = () =>
            result.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.File);

        private static PreloadedDataVerifier preloadedDataVerifier;
        private static IEnumerable<PreloadedDataVerificationError> result;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
    }
}
