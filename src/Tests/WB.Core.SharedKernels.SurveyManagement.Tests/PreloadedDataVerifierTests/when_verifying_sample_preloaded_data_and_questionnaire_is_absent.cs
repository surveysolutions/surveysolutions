﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.PreloadedDataVerifierTests
{
    internal class when_verifying_sample_preloaded_data_and_questionnaire_is_absent : PreloadedDataVerifierTestContext
    {
        Establish context = () =>
        {
            preloadedDataVerifier = CreatePreloadedDataVerifier();
        };

        Because of =
            () =>
                result =
                    preloadedDataVerifier.VerifySample(Guid.NewGuid(), 1, new PreloadedDataByFile("1", "1", new string[0], new string[0][]));

        It should_result_has_1_error = () =>
            result.Count().ShouldEqual(1);

        It should_return_single_PL0001_error = () =>
            result.First().Code.ShouldEqual("PL0001");

        private static PreloadedDataVerifier preloadedDataVerifier;
        private static IEnumerable<PreloadedDataVerificationError> result;
    }
}
