using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Templates;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Export;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadingTemplateServiceTests
{
    internal class when_questionnaire_has_2_levels_and_GetFilePathToPreloadingTemplate_called : PreloadingTemplateServiceTestContext
    {
        Establish context = () =>
        {
            exportedDataFormatter = new Mock<ITabularFormatExportService>();
            fileSystemAccessor = CreateIFileSystemAccessorMock();
            fileSystemAccessor.Setup(x => x.GetFilesInDirectory(Moq.It.IsAny<string>(), Moq.It.IsAny<bool>())).Returns(new[] { "1.tab" });

            var exportFileNameService = Mock.Of<IExportFileNameService>(x => 
                x.GetFileNameForBatchUploadByQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>()) == "template.zip");

            assignmentImportTemplateGenerator = CreatePreloadingTemplateService(
                fileSystemAccessor.Object,
                tabularFormatExportService: exportedDataFormatter.Object,
                exportFileNameService: exportFileNameService);
        };

        Because of = () => result = assignmentImportTemplateGenerator.GetFilePathToPreloadingTemplate(questionnaireId, 1);

        It should_return_not_null_result = () =>
           result.ShouldNotBeNull();

        It should_only_create_template_for_preload_once = () =>
            exportedDataFormatter.Verify(x => x.CreateHeaderStructureForPreloadingForQuestionnaire(new QuestionnaireIdentity(questionnaireId, 1), Moq.It.IsAny<string>()), Times.Once);

        private static AssignmentImportTemplateGenerator assignmentImportTemplateGenerator;
        private static string result;
        private static Mock<IFileSystemAccessor> fileSystemAccessor;
        private static Mock<ITabularFormatExportService> exportedDataFormatter;
        private static readonly Guid questionnaireId = Guid.NewGuid();
    }
}
