using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_panel_preloaded_data_and_questionnaire_is_absent : PreloadedDataVerifierTestContext
    {
        Establish context = () =>
        {
            importDataVerifier = CreatePreloadedDataVerifier();
        };

        Because of = () => importDataVerifier.VerifyPanelFiles(Guid.NewGuid(), 1, new PreloadedDataByFile[0], status);

        It should_result_has_1_error = () =>
            status.VerificationState.Errors.Count().ShouldEqual(1);

        It should_return_single_PL0001_error = () =>
            status.VerificationState.Errors.First().Code.ShouldEqual("PL0024");

        private static ImportDataVerifier importDataVerifier;
    }
}
