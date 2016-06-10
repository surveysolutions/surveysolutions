﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadingTemplateServiceTests
{
    internal class when_questionnaire_is_missing_and_GetFilePathToPreloadingTemplate_called : PreloadingTemplateServiceTestContext
    {
        Establish context = () =>
        {
            var exportFileNameService = Mock.Of<IExportFileNameService>(x =>
                x.GetFileNameForBatchUploadByQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>()) == "template.zip");
            preloadingTemplateService = CreatePreloadingTemplateService(exportFileNameService: exportFileNameService);
        };

        Because of = () => result = preloadingTemplateService.GetFilePathToPreloadingTemplate(Guid.NewGuid(), 1);

        It should_return_be_null = () =>
           result.ShouldBeNull();

        private static PreloadingTemplateService preloadingTemplateService;
        private static string result;
    }
}
