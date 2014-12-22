using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadingTemplateServiceTests
{
    internal class when_questionnaire_is_missing_and_GetFilePathToPreloadingTemplate_called : PreloadingTemplateServiceTestContext
    {
        Establish context = () =>
        {
            preloadingTemplateService = CreatePreloadingTemplateService();
        };

        Because of = () => result = preloadingTemplateService.GetFilePathToPreloadingTemplate(Guid.NewGuid(), 1);

        It should_return_be_null = () =>
           result.ShouldBeNull();

        private static PreloadingTemplateService preloadingTemplateService;
        private static string result;
    }
}
