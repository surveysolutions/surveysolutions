using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Templates;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadingTemplateServiceTests
{
    internal class when_questionnaire_is_missing_and_GetFilePathToPreloadingTemplate_called : PreloadingTemplateServiceTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var exportFileNameService = Mock.Of<IExportFileNameService>(x =>
                x.GetFileNameForBatchUploadByQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>()) == "template.zip");
            assignmentImportTemplateGenerator = CreatePreloadingTemplateService(exportFileNameService: exportFileNameService);
            BecauseOf();
        }

        public void BecauseOf() => result = assignmentImportTemplateGenerator.GetFilePathToPreloadingTemplate(Guid.NewGuid(), 1);

        [NUnit.Framework.Test] public void should_return_be_null () =>
           result.Should().BeNull();

        private static AssignmentImportTemplateGenerator assignmentImportTemplateGenerator;
        private static string result;
    }
}
