﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.PreloadingTemplateServiceTests
{
    internal class when_archive_were_build_before_GetFilePathToPreloadingTemplate_called : PreloadingTemplateServiceTestContext
    {
        Establish context = () =>
        {
            fileSystemAccessor = CreateIFileSystemAccessorMock();
            fileSystemAccessor.Setup(x => x.IsFileExists(Moq.It.IsAny<string>())).Returns(true);
            dataFileExportService = CreateIDataFileExportServiceMock();
            questionnaireExportStructure = CreateQuestionnaireExportStructure(2);
            preloadingTemplateService = CreatePreloadingTemplateService(dataFileExportService.Object, questionnaireExportStructure, fileSystemAccessor.Object);
        };

        Because of = () => result = preloadingTemplateService.GetFilePathToPreloadingTemplate(questionnaireId, 1);

        It should_result_be_not_null = () =>
           result.ShouldNotBeNull();

        It should_return_valid_archive_name = () =>
            result.ShouldEndWith(string.Format("template_{0}_v{1}.zip", questionnaireId.FormatGuid(), 1));

        It should_header_be_created_zero_times = () =>
           dataFileExportService.Verify(x => x.CreateHeader(Moq.It.IsAny<HeaderStructureForLevel>(), Moq.It.IsAny<string>()), Times.Never);

        private static PreloadingTemplateService preloadingTemplateService;
        private static string result;
        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static Mock<IFileSystemAccessor> fileSystemAccessor;
        private static Mock<IDataFileExportService> dataFileExportService;
        private static Guid questionnaireId = Guid.NewGuid();
    }
}
